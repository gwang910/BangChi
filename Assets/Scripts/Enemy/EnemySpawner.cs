using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public static EnemySpawner Instance;

    [Header("Target")]
    public Transform player;                 // 플레이어 Transform
    public LayerMask groundMask = ~0;        // 바닥 레이어(없으면 무시)

    [Header("Spawn Source")]
    public bool useSpawnPoints = true;       // true면 spawnPoints 중 랜덤, false면 플레이어 주변 랜덤
    public Transform[] spawnPoints;          // 스폰 포인트들
    public float ringMin = 8f, ringMax = 18f;// 플레이어 주변 랜덤 링 반경(포인트 미사용 시)

    [Header("Enemy")]
    public GameObject enemyPrefab;           // 현재 스테이지 적 프리팹
    public int targetAliveCount = 5;         // 항상 유지할 적 수
    public float respawnDelay = 1.0f;        // 죽은 뒤 리스폰 딜레이
    public float minSeparation = 2.0f;       // 서로 겹치지 않게 최소 간격

    // 간단한 풀
    Queue<GameObject> _pool = new Queue<GameObject>();
    readonly List<GameObject> _alive = new List<GameObject>();
    readonly List<Vector3> _occupied = new List<Vector3>(); // 최근 스폰 위치들(겹침 방지)

    void Start()
    {
        if (!player) player = GameObject.FindGameObjectWithTag("Player")?.transform;
        // 시작 시 목표 수만큼 채우기
        for (int i = 0; i < targetAliveCount; i++) SpawnOne();
    }

    void Update()
    {
        // 안전 장치: 전투/버그로 사라졌으면 다시 채움
        while (_alive.Count < targetAliveCount) SpawnOne();
    }

    // ---------- 스폰 로직 ----------
    void SpawnOne()
    {
        Vector3 pos;
        if (!TryGetSpawnPosition(out pos)) return;

        var go = Instantiate(enemyPrefab, pos + Vector3.up, Quaternion.identity); // 살짝 위에서
        SnapToGround(go);
        go.transform.SetPositionAndRotation(pos, Quaternion.identity);
        go.SetActive(true);

        // 체력/죽음 이벤트 구독
        var hp = go.GetComponent<Health>();
        if (hp == null) hp = go.AddComponent<Health>(); // 없으면 임시 부여
        hp.OnDead -= () => OnEnemyDead(go); // 중복 제거
        hp.OnDead += () => OnEnemyDead(go);

        _alive.Add(go);
        _occupied.Add(pos);
    }

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
        // N회 시도 후 포기
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
                // 플레이어 주변 '링'에서 랜덤
                var dir = Random.insideUnitCircle.normalized;
                float r = Random.Range(ringMin, ringMax);
                candidate = player.position + new Vector3(dir.x, 0, dir.y) * r;
            }

            // 바닥에 투영(Raycast로 Y보정)
            Vector3 grounded = candidate;
            if (Physics.Raycast(candidate + Vector3.up * 30f, Vector3.down, out var hit, 100f, groundMask))
                grounded = hit.point;

            // 최소 간격 확보
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

    void OnEnemyDead(GameObject go)
    {
        // 즉시 리스트에서 제거하고, 일정 후 리스폰
        _alive.Remove(go);
        // 점유 좌표 정리
        _occupied.RemoveAll(v => (v - go.transform.position).sqrMagnitude < 0.01f);

        // GameManager 연동(있으면 경험치/골드 반영)
        try { GameManager.Instance.OnEnemyKilled(); } catch { }

        StartCoroutine(CoRecycleAndRespawn(go));
    }

    IEnumerator CoRecycleAndRespawn(GameObject go)
    {
        // 사망 연출을 위해 잠깐 대기 후 비활성 & 풀 반환
        yield return new WaitForSeconds(respawnDelay);
        ReturnToPool(go);

        // targetAliveCount 유지: Update가 보충하지만, 즉시 1기 스폰해도 됨
        if (_alive.Count < targetAliveCount) SpawnOne();
    }

    // ---------- 간단 풀 ----------
    GameObject GetFromPool()
    {
        GameObject go = null;
        while (_pool.Count > 0 && go == null)
        {
            go = _pool.Dequeue();
        }
        if (go == null)
        {
            go = Instantiate(enemyPrefab);
            // 레이어/태그 등 초기화 필요 시 여기서
        }
        return go;
    }
    void ReturnToPool(GameObject go)
    {
        if (!go) return;
        Destroy(go);
    }

    public static void ReplaceEnemies(GameObject enemyPrefab)
    {
        if (Instance == null) return;
        // 기존 삭제
        foreach (var e in Instance._alive) if (e) Destroy(e);
        Instance._alive.Clear();
        // 새로 스폰
        foreach (var p in Instance.spawnPoints)
        {
            var go = Instantiate(enemyPrefab, p.position, p.rotation);
            // 처치 시 GameManager에 알리기
            var hp = go.GetComponent<Health>();
            hp.OnDead += () => { GameManager.Instance.OnEnemyKilled(); };
            Instance._alive.Add(go);
        }
    }

}
