using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public static Inventory Instance;
    public List<StatUpgradeSO> purchased = new List<StatUpgradeSO>(); // 구매 내역

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public bool Buy(StatUpgradeSO item)
    {
        var gm = GameManager.Instance;
        if (gm.gold < item.cost) return false;

        gm.gold -= item.cost;
        Apply(item);
        purchased.Add(item);

        gm.RaiseStatsChanged();
        return true;
    }

    public void Apply(StatUpgradeSO item)
    {
        var s = GameManager.Instance.stats;
        switch (item.stat)
        {
            case StatType.MaxHP:
                s.maxHp += item.amount;
                s.hp = Mathf.Min(s.hp + item.amount, s.maxHp);
                break;
            case StatType.MaxMP:
                s.maxMp += item.amount;
                s.mp = Mathf.Min(s.mp + item.amount, s.maxMp);
                break;
            case StatType.ATK: s.atk += item.amount; break;
            case StatType.DEF: s.def += item.amount; break;
            case StatType.DEX: s.dex += item.amount; break;
        }
    }
}
