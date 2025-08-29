using UnityEngine;

[CreateAssetMenu(menuName = "RPG/StageConfigSO")]
public class StageConfigSO : ScriptableObject
{
    public int stageNumber;
    public GameObject enemyPrefab;     // �� ������������ ������ �� ������
    public int expPerKill = 20;
    public int goldPerKill = 5;
    public int expToNext = 100;        // ���� ���������� �� ����ġ �䱸��
}
