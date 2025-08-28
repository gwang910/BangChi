using UnityEngine;
using System.Collections.Generic;

public class SpawnManager : MonoBehaviour
{
    public static SpawnManager I;
    public Transform[] spawnPoints;      // 적 스폰 위치들
    List<GameObject> _enemies = new List<GameObject>();

    void Awake() { I = this; }

    public static void ReplaceEnemies(GameObject enemyPrefab)
    {
        if (I == null) return;
        // 기존 삭제
        foreach (var e in I._enemies) if (e) Destroy(e);
        I._enemies.Clear();
        // 새로 스폰
        foreach (var p in I.spawnPoints)
        {
            var go = Instantiate(enemyPrefab, p.position, p.rotation);
            // 처치 시 GameManager에 알리기
            var hp = go.GetComponent<Health>();
            hp.OnDead += () => { GameManager.Instance.OnEnemyKilled(); };
            I._enemies.Add(go);
        }
    }

    // 씬 시작 시 초기 스폰
    void Start() { ReplaceEnemies(GameManager.Instance.CurStage.enemyPrefab); }
}
