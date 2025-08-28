using UnityEngine;

public enum StatType { MaxHP, MaxMP, ATK, DEF, DEX }

[CreateAssetMenu(menuName = "RPG/Stat Upgrade")]
public class StatUpgradeSO : ScriptableObject
{
    public string displayName;
    public StatType stat;
    public int amount = 10;
    public int cost = 50;
    [TextArea] public string description;
}
