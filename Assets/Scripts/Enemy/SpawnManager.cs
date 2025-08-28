using UnityEngine;
using System.Collections.Generic;

public class SpawnManager : MonoBehaviour
{
    public static SpawnManager I;
    public Transform[] spawnPoints;      // �� ���� ��ġ��
    List<GameObject> _enemies = new List<GameObject>();

    void Awake() { I = this; }

    public static void ReplaceEnemies(GameObject enemyPrefab)
    {
        if (I == null) return;
        // ���� ����
        foreach (var e in I._enemies) if (e) Destroy(e);
        I._enemies.Clear();
        // ���� ����
        foreach (var p in I.spawnPoints)
        {
            var go = Instantiate(enemyPrefab, p.position, p.rotation);
            // óġ �� GameManager�� �˸���
            var hp = go.GetComponent<Health>();
            hp.OnDead += () => { GameManager.Instance.OnEnemyKilled(); };
            I._enemies.Add(go);
        }
    }

    // �� ���� �� �ʱ� ����
    void Start() { ReplaceEnemies(GameManager.Instance.CurStage.enemyPrefab); }
}
