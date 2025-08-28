using UnityEngine;
using UnityEngine.UI;

public class HUD : MonoBehaviour
{
    public Slider hpBar, mpBar, expBar;
    public Text stageText, goldText;

    Health _playerHP;

    void Start()
    {
        _playerHP = GameObject.FindGameObjectWithTag("Player").GetComponent<Health>();
        _playerHP.OnChanged += RefreshHP;
        GameManager.Instance.OnStatsChanged += RefreshAll;

        RefreshAll();
    }

    void RefreshHP()
    {
        if (!hpBar) return;
        hpBar.maxValue = _playerHP.maxHP;
        hpBar.value = _playerHP.currentHP;
    }

    void RefreshAll()
    {
        RefreshHP();
        if (mpBar) { mpBar.maxValue = GameManager.Instance.playerBase.maxMP; mpBar.value = GameManager.Instance.playerBase.maxMP; } // MP 시스템 추가 시 연동
        var st = GameManager.Instance.CurStage;
        if (expBar) { expBar.maxValue = st.expToNext; expBar.value = GameManager.Instance.stats.exp; }
        if (stageText) stageText.text = $"Stage {st.stageNumber:00}";
        if (goldText) goldText.text = $"{GameManager.Instance.gold}";
    }
}
