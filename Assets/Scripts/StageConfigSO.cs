using UnityEngine;

[CreateAssetMenu(menuName = "RPG/StageConfigSO")]
public class StageConfigSO : ScriptableObject
{
    public int stageNumber;
    public GameObject enemyPrefab;     // 이 스테이지에서 스폰할 적 프리팹
    public int expPerKill = 20;
    public int goldPerKill = 5;
    public int expToNext = 100;        // 다음 스테이지로 갈 경험치 요구량
}
