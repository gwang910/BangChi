using UnityEngine;

[CreateAssetMenu(menuName = "RPG/CharacterStats")]
public class CharacterStatsSO : ScriptableObject
{
    public int maxHP = 100;
    public int maxMP = 50;
    public int atk = 10;
    public int def = 5;
    public int dex = 7;
}

