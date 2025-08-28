using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class HUD : MonoBehaviour
{
    public Slider hpBar, mpBar, expBar;
    public Text stageText, goldText;

    void OnEnable() { StartCoroutine(Bind()); }
    void OnDisable() { if (GameManager.Instance) GameManager.Instance.OnStatsChanged -= Refresh; }

    IEnumerator Bind()
    {
        while (GameManager.Instance == null) yield return null;   // ���� ���
        GameManager.Instance.OnStatsChanged -= Refresh;           // �ߺ� ����
        GameManager.Instance.OnStatsChanged += Refresh;
        Refresh();                                                // �ʱⰪ ��� �ݿ�
    }

    void Refresh()
    {
        var gm = GameManager.Instance; if (gm == null) return;
        if (hpBar) { hpBar.maxValue = gm.stats.maxHp; hpBar.value = gm.stats.hp; }
        if (mpBar) { mpBar.maxValue = gm.stats.maxMp; mpBar.value = gm.stats.mp; }
        if (expBar) { expBar.maxValue = Mathf.Max(1, gm.stats.maxExp); expBar.value = Mathf.Min(gm.stats.exp, gm.stats.maxExp); }
        if (stageText) stageText.text = $"Stage {gm.stats.stage:00}";
        if (goldText) goldText.text = gm.gold.ToString();
    }
}
