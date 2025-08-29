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
    public GameObject enemyPrefab;      // ���� �⺻ ������
    GameObject _nextPrefab;             // ���� �ܰ� ������(�����Ǹ� ���� ��ü���� ��ü)
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
        // ������ ���� ���� ���������� ä�� (�ߺ� ���� ����)
        ForceFillWith(enemyPrefab);
    }

    void Update()
    {
        // ����/���׷� ��ü ���� �ٸ� �ڵ� ����(���� �⺻ ������ ����)
        while (_alive.Count < targetAliveCount)
            SpawnOneAuto(enemyPrefab);
    }

    // ----------------- �ܺ� API -----------------

    // GameManager�� �������� ���� �� ȣ��: ���� �״� ������ �� ���������� ������
    public void SetNextPrefab(GameObject p)
    {
        _nextPrefab = p;
    }

    // ó��/���� ��ü: ���� �ı� �� ���� ���������� ä��
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

    // ----------------- ���� ���� -----------------

    // ��ġ �ڵ� �����ؼ� 1�� ����
    void SpawnOneAuto(GameObject prefab)
    {
        if (TryGetSpawnPosition(out var pos))
            SpawnAt(prefab, pos, Quaternion.identity);
    }

    // ��Ȯ�� ��ġ/ȸ������ ����(����Ʈ��)
    void SpawnAt(GameObject prefab, Vector3 pos, Quaternion rot)
    {
        var go = Instantiate(prefab, pos, rot);
        SnapToGround(go);

        // ���� �̺�Ʈ �� ���� ��ġ ������
        var hp = go.GetComponent<Health>();
        if (!hp) hp = go.AddComponent<Health>();

        // ���� �ڵ鷯�� ����� ���� ����(�ʿ� �� ���� ����)
        System.Action onDead = null;
        onDead = () =>
        {
            hp.OnDead -= onDead;   // �����ϰ� ����
            OnEnemyDead(go, pos, rot);
        };
        hp.OnDead += onDead;

        _alive.Add(go);
        _occupied.Add(pos);
    }

    void OnEnemyDead(GameObject go, Vector3 pos, Quaternion rot)
    {
        _alive.Remove(go);
        // ���� ��ǥ ����
        _occupied.RemoveAll(v => (v - pos).sqrMagnitude < 0.01f);

        // ���� ó��
        try { GameManager.Instance?.OnEnemyKilled(); } catch { }

        // ��� ������ ���� ��� ��� �� ���� �ڸ� ������
        StartCoroutine(CoRespawnAt(pos, rot));
        Destroy(go);
    }

    IEnumerator CoRespawnAt(Vector3 pos, Quaternion rot)
    {
        yield return new WaitForSeconds(respawnDelay);

        // ���� �������� ������ ������ �װɷ�, �ƴϸ� ���� �⺻ ����������
        var prefabToUse = _nextPrefab ? _nextPrefab : enemyPrefab;
        SpawnAt(prefabToUse, pos, rot);

        // ������ �̹� next ���������� ��ü�ƴ��� �˻� �� �⺻ ������ �°�
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

    // ----------------- ���� -----------------

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

            // �ٴ� ����
            var grounded = candidate;
            if (Physics.Raycast(candidate + Vector3.up * 30f, Vector3.down, out var hit, 100f, groundMask))
                grounded = hit.point;

            // �ּ� ����
            bool ok = true;
            foreach (var occ in _occupied)
                if ((occ - grounded).sqrMagnitude < minSeparation * minSeparation) { ok = false; break; }

            // �÷��̾�� �ʹ� ������ �ʰ� (�ɼ�)
            if (player && (player.position - grounded).sqrMagnitude < (ringMin * 0.8f) * (ringMin * 0.8f))
                ok = false;

            if (ok) { pos = grounded; return true; }
        }
        pos = transform.position;
        return true;
    }

    bool SamePrefab(GameObject instance, GameObject prefab)
    {
        // ��Ȯ �񱳸� ���ϸ� �� �����տ� EnemyKind(level) ������Ʈ�� �ΰ� �� ������ ���ϼ���.
        return instance.name.StartsWith(prefab.name);
    }
}
