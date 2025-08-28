using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public static EnemySpawner Instance;

    [Header("Target")]
    public Transform player;                 // �÷��̾� Transform
    public LayerMask groundMask = ~0;        // �ٴ� ���̾�(������ ����)

    [Header("Spawn Source")]
    public bool useSpawnPoints = true;       // true�� spawnPoints �� ����, false�� �÷��̾� �ֺ� ����
    public Transform[] spawnPoints;          // ���� ����Ʈ��
    public float ringMin = 8f, ringMax = 18f;// �÷��̾� �ֺ� ���� �� �ݰ�(����Ʈ �̻�� ��)

    [Header("Enemy")]
    public GameObject enemyPrefab;           // ���� �������� �� ������
    public int targetAliveCount = 5;         // �׻� ������ �� ��
    public float respawnDelay = 1.0f;        // ���� �� ������ ������
    public float minSeparation = 2.0f;       // ���� ��ġ�� �ʰ� �ּ� ����

    // ������ Ǯ
    Queue<GameObject> _pool = new Queue<GameObject>();
    readonly List<GameObject> _alive = new List<GameObject>();
    readonly List<Vector3> _occupied = new List<Vector3>(); // �ֱ� ���� ��ġ��(��ħ ����)

    void Start()
    {
        if (!player) player = GameObject.FindGameObjectWithTag("Player")?.transform;
        // ���� �� ��ǥ ����ŭ ä���
        for (int i = 0; i < targetAliveCount; i++) SpawnOne();
    }

    void Update()
    {
        // ���� ��ġ: ����/���׷� ��������� �ٽ� ä��
        while (_alive.Count < targetAliveCount) SpawnOne();
    }

    // ---------- ���� ���� ----------
    void SpawnOne()
    {
        Vector3 pos;
        if (!TryGetSpawnPosition(out pos)) return;

        var go = Instantiate(enemyPrefab, pos + Vector3.up, Quaternion.identity); // ��¦ ������
        SnapToGround(go);
        go.transform.SetPositionAndRotation(pos, Quaternion.identity);
        go.SetActive(true);

        // ü��/���� �̺�Ʈ ����
        var hp = go.GetComponent<Health>();
        if (hp == null) hp = go.AddComponent<Health>(); // ������ �ӽ� �ο�
        hp.OnDead -= () => OnEnemyDead(go); // �ߺ� ����
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
        // Nȸ �õ� �� ����
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
                // �÷��̾� �ֺ� '��'���� ����
                var dir = Random.insideUnitCircle.normalized;
                float r = Random.Range(ringMin, ringMax);
                candidate = player.position + new Vector3(dir.x, 0, dir.y) * r;
            }

            // �ٴڿ� ����(Raycast�� Y����)
            Vector3 grounded = candidate;
            if (Physics.Raycast(candidate + Vector3.up * 30f, Vector3.down, out var hit, 100f, groundMask))
                grounded = hit.point;

            // �ּ� ���� Ȯ��
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

    void OnEnemyDead(GameObject go)
    {
        // ��� ����Ʈ���� �����ϰ�, ���� �� ������
        _alive.Remove(go);
        // ���� ��ǥ ����
        _occupied.RemoveAll(v => (v - go.transform.position).sqrMagnitude < 0.01f);

        // GameManager ����(������ ����ġ/��� �ݿ�)
        try { GameManager.Instance.OnEnemyKilled(); } catch { }

        StartCoroutine(CoRecycleAndRespawn(go));
    }

    IEnumerator CoRecycleAndRespawn(GameObject go)
    {
        // ��� ������ ���� ��� ��� �� ��Ȱ�� & Ǯ ��ȯ
        yield return new WaitForSeconds(respawnDelay);
        ReturnToPool(go);

        // targetAliveCount ����: Update�� ����������, ��� 1�� �����ص� ��
        if (_alive.Count < targetAliveCount) SpawnOne();
    }

    // ---------- ���� Ǯ ----------
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
            // ���̾�/�±� �� �ʱ�ȭ �ʿ� �� ���⼭
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
        // ���� ����
        foreach (var e in Instance._alive) if (e) Destroy(e);
        Instance._alive.Clear();
        // ���� ����
        foreach (var p in Instance.spawnPoints)
        {
            var go = Instantiate(enemyPrefab, p.position, p.rotation);
            // óġ �� GameManager�� �˸���
            var hp = go.GetComponent<Health>();
            hp.OnDead += () => { GameManager.Instance.OnEnemyKilled(); };
            Instance._alive.Add(go);
        }
    }

}
