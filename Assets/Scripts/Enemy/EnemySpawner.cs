using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public static EnemySpawner Instance;

    [Header("Target")]
    public Transform player;
    public LayerMask groundMask = ~0;

    [Header("Spawn Source")]
    public bool useSpawnPoints = true;
    public Transform[] spawnPoints;
    public float ringMin = 8f, ringMax = 18f;

    [Header("Enemy")]
    public GameObject enemyPrefab;      // 현재 기본 프리팹
    GameObject _nextPrefab;             // 다음 단계 프리팹(지정되면 죽은 개체부터 교체)
    public int targetAliveCount = 5;
    public float respawnDelay = 1.0f;
    public float minSeparation = 2.0f;

    readonly List<GameObject> _alive = new List<GameObject>();
    readonly List<Vector3> _occupied = new List<Vector3>();

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        if (!player) player = GameObject.FindGameObjectWithTag("Player")?.transform;
        // 시작은 전부 현재 프리팹으로 채움 (중복 스폰 금지)
        ForceFillWith(enemyPrefab);
    }

    void Update()
    {
        // 전투/버그로 개체 수가 줄면 자동 보충(현재 기본 프리팹 기준)
        while (_alive.Count < targetAliveCount)
            SpawnOneAuto(enemyPrefab);
    }

    // ----------------- 외부 API -----------------

    // GameManager가 스테이지 오를 때 호출: 이후 죽는 적부터 이 프리팹으로 리스폰
    public void SetNextPrefab(GameObject p)
    {
        _nextPrefab = p;
    }

    // 처음/강제 교체: 전부 파괴 후 지정 프리팹으로 채움
    public void ForceFillWith(GameObject prefab)
    {
        foreach (var e in _alive) if (e) Destroy(e);
        _alive.Clear();
        _occupied.Clear();

        enemyPrefab = prefab;

        if (useSpawnPoints && spawnPoints != null && spawnPoints.Length > 0)
        {
            foreach (var sp in spawnPoints)
                SpawnAt(prefab, sp.position, sp.rotation);
        }
        else
        {
            for (int i = 0; i < targetAliveCount; i++)
                SpawnOneAuto(prefab);
        }
    }

    // ----------------- 스폰 로직 -----------------

    // 위치 자동 선정해서 1기 스폰
    void SpawnOneAuto(GameObject prefab)
    {
        if (TryGetSpawnPosition(out var pos))
            SpawnAt(prefab, pos, Quaternion.identity);
    }

    // 정확한 위치/회전으로 스폰(포인트용)
    void SpawnAt(GameObject prefab, Vector3 pos, Quaternion rot)
    {
        var go = Instantiate(prefab, pos, rot);
        SnapToGround(go);

        // 죽음 이벤트 → 동일 위치 리스폰
        var hp = go.GetComponent<Health>();
        if (!hp) hp = go.AddComponent<Health>();

        // 로컬 핸들러를 만들어 참조 보유(필요 시 해제 가능)
        System.Action onDead = null;
        onDead = () =>
        {
            hp.OnDead -= onDead;   // 안전하게 해제
            OnEnemyDead(go, pos, rot);
        };
        hp.OnDead += onDead;

        _alive.Add(go);
        _occupied.Add(pos);
    }

    void OnEnemyDead(GameObject go, Vector3 pos, Quaternion rot)
    {
        _alive.Remove(go);
        // 점유 좌표 정리
        _occupied.RemoveAll(v => (v - pos).sqrMagnitude < 0.01f);

        // 보상 처리
        try { GameManager.Instance?.OnEnemyKilled(); } catch { }

        // 사망 연출을 위해 잠깐 대기 후 같은 자리 리스폰
        StartCoroutine(CoRespawnAt(pos, rot));
        Destroy(go);
    }

    IEnumerator CoRespawnAt(Vector3 pos, Quaternion rot)
    {
        yield return new WaitForSeconds(respawnDelay);

        // 다음 프리팹이 지정돼 있으면 그걸로, 아니면 현재 기본 프리팹으로
        var prefabToUse = _nextPrefab ? _nextPrefab : enemyPrefab;
        SpawnAt(prefabToUse, pos, rot);

        // 전원이 이미 next 프리팹으로 교체됐는지 검사 → 기본 프리팹 승격
        if (_nextPrefab)
        {
            bool allUpgraded = true;
            foreach (var e in _alive)
            {
                if (!e) continue;
                if (!SamePrefab(e, _nextPrefab)) { allUpgraded = false; break; }
            }
            if (allUpgraded)
            {
                enemyPrefab = _nextPrefab;
                _nextPrefab = null;
            }
        }
    }

    // ----------------- 보조 -----------------

    void SnapToGround(GameObject go)
    {
        var start = go.transform.position + Vector3.up * 3f;
        if (Physics.Raycast(start, Vector3.down, out var hit, 10f, groundMask))
        {
            var p = go.transform.position; p.y = hit.point.y; go.transform.position = p;
        }
    }

    bool TryGetSpawnPosition(out Vector3 pos)
    {
        for (int tries = 0; tries < 20; tries++)
        {
            Vector3 candidate;
            if (useSpawnPoints && spawnPoints != null && spawnPoints.Length > 0)
            {
                var p = spawnPoints[Random.Range(0, spawnPoints.Length)];
                candidate = p.position;
            }
            else
            {
                if (!player) { pos = transform.position; return true; }
                var dir = Random.insideUnitCircle.normalized;
                float r = Random.Range(ringMin, ringMax);
                candidate = player.position + new Vector3(dir.x, 0, dir.y) * r;
            }

            // 바닥 보정
            var grounded = candidate;
            if (Physics.Raycast(candidate + Vector3.up * 30f, Vector3.down, out var hit, 100f, groundMask))
                grounded = hit.point;

            // 최소 간격
            bool ok = true;
            foreach (var occ in _occupied)
                if ((occ - grounded).sqrMagnitude < minSeparation * minSeparation) { ok = false; break; }

            // 플레이어와 너무 가깝지 않게 (옵션)
            if (player && (player.position - grounded).sqrMagnitude < (ringMin * 0.8f) * (ringMin * 0.8f))
                ok = false;

            if (ok) { pos = grounded; return true; }
        }
        pos = transform.position;
        return true;
    }

    bool SamePrefab(GameObject instance, GameObject prefab)
    {
        // 정확 비교를 원하면 각 프리팹에 EnemyKind(level) 컴포넌트를 두고 그 값으로 비교하세요.
        return instance.name.StartsWith(prefab.name);
    }
}
