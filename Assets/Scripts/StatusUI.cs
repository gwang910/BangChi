using UnityEngine;
using UnityEngine.UI;

public class StatusUI : MonoBehaviour
{
    public Text idText;
    public Text stageText;
    public Text hpText, mpText, expText;
    public Text atkText, defText, dexText;

    void OnEnable()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnStatsChanged += Refresh;
        Refresh();
    }
    void OnDisable()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnStatsChanged -= Refresh;
    }

    public void Refresh()
    {
        var gm = GameManager.Instance;
        if (gm == null) return;

        var s = gm.stats;
        if (idText) idText.text = $"{gm.userId}";
        if (stageText) stageText.text = $"Stage {s.stage:00}";
        if (hpText) hpText.text = $"HP {s.hp:000} / {s.maxHp:000}";
        if (mpText) mpText.text = $"MP {s.mp:000} / {s.maxMp:000}";
        if (expText) expText.text = $"EXP {s.exp:000} / {s.maxExp:000}";
        if (atkText) atkText.text = $"ATK {s.atk:00}";
        if (defText) defText.text = $"DEF {s.def:00}";
        if (dexText) dexText.text = $"DEX {s.dex:00}";
    }

}
