using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;

[Serializable]
public class PlayerStats
{
    public int stage = 0;
    public int hp = 100, maxHp = 100;
    public int mp = 50, maxMp = 50;
    public int exp = 0, maxExp = 100;
    public int atk = 10, def = 5, dex = 7;
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Data (optional)")]
    public CharacterStatsSO playerBase;     // �⺻ ���� SO
    public StageConfigSO[] stages;          // �������� ���� ����Ʈ

    [Header("Runtime")]
    public string userId = "Guest";
    public PlayerStats stats = new PlayerStats();

    public int gold = 0;

    public event Action OnStatsChanged;

    string _userKeyPrefix => $"{userId}";   // ����ں� Ű prefix

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        // �α��ο��� ������ ID ����
        userId = PlayerPrefs.GetString("userId", "Guest");

        if ((stages == null || stages.Length == 0))
        {
            var loaded = Resources.LoadAll<StageConfigSO>("Stages");
            if (loaded != null && loaded.Length > 0)
                stages = loaded.OrderBy(s => s.stageNumber).ToArray();
        }

        ApplyPlayerBase();   // SO�� ������ �⺻ �ɷ�ġ �ݿ�
        LoadProgress();      // ����ں� ���� �ε�
        ClampStageAndExp();  // �ε尪�� ���� �������� �䱸ġ�� ����

        // �ʱ� ���� ��ε�ĳ��Ʈ
        OnStatsChanged?.Invoke();

        // ���� �� ���� ���������� ������ ��ü(������)
        ReplaceEnemiesForCurrentStage();
    }

    void ApplyPlayerBase()
    {
        if (playerBase == null) return;

        stats.maxHp = playerBase.maxHP;
        stats.hp = Mathf.Min(stats.hp, stats.maxHp);

        stats.maxMp = playerBase.maxMP;
        stats.mp = Mathf.Min(stats.mp, stats.maxMp);

        stats.atk = playerBase.atk;
        stats.def = playerBase.def;
        stats.dex = playerBase.dex;
    }

    // ====== ���� �������� ���� ��ȸ ======
    public StageConfigSO CurStage
    {
        get
        {
            if (stages == null || stages.Length == 0) return null;
            int idx = Mathf.Clamp(stats.stage, 0, stages.Length - 1);
            return stages[idx];
        }
    }

    // ���� �������� �䱸ġ/�� �ݿ�
    void ClampStageAndExp()
    {
        var st = CurStage;
        if (st != null) stats.maxExp = Mathf.Max(1, st.expToNext);
        stats.exp = Mathf.Clamp(stats.exp, 0, stats.maxExp);
    }

    // ====== ����/����ġ ======
    // �� óġ�� ȣ���ϸ� ����ġ/��� �ݿ� �� �ʿ��ϸ� �������� ���
    public void OnEnemyKilled()
    {
        var st = CurStage;
        int addExp = st ? st.expPerKill : 10;
        int addGold = st ? st.goldPerKill : 1;

        gold += addGold;
        stats.exp += addExp;

        while (stats.exp >= stats.maxExp)
        {
            stats.exp -= stats.maxExp;
            stats.stage++;
            // �������� ��� �� �� �䱸ġ �ݿ�
            var next = CurStage;
            stats.maxExp = next ? Mathf.Max(1, next.expToNext) : stats.maxExp;
        }

        SaveProgress();
        OnStatsChanged?.Invoke();

        // �� ���� ������ ��ü (SpawnManager�� ������)
        ReplaceEnemiesForCurrentStage();
    }

    // ��ġ�� ���� ������ �� �޼���� ȣ��
    public void AddExp(int amount)
    {
        stats.exp += Mathf.Max(0, amount);
        while (stats.exp >= stats.maxExp)
        {
            stats.exp -= stats.maxExp;
            stats.stage++;
            var next = CurStage;
            stats.maxExp = next ? Mathf.Max(1, next.expToNext) : stats.maxExp;
        }
        SaveProgress();
        OnStatsChanged?.Invoke();
        ReplaceEnemiesForCurrentStage();
    }
    public void TakeDamage(int dmg)
    {
        stats.hp = Mathf.Max(0, stats.hp - Mathf.Max(0, dmg));
        OnStatsChanged?.Invoke();
    }
    public void Heal(int amt)
    {
        stats.hp = Mathf.Min(stats.maxHp, stats.hp + Mathf.Max(0, amt));
        OnStatsChanged?.Invoke();
    }

    public void GoToStage(int targetStage)
    {
        // �÷��̾ ������ �ִ� ������������ ���
        int reached = PlayerPrefs.GetInt($"{_userKeyPrefix}_reachedStage", 0);
        int maxAllowed = Mathf.Max(reached, stats.stage);
        stats.stage = Mathf.Clamp(targetStage, 0, maxAllowed);

        // �̵� �� ����ġ�� �ʱ�ȭ
        stats.exp = 0;
        ClampStageAndExp();

        SaveProgress();
        OnStatsChanged?.Invoke();
        ReplaceEnemiesForCurrentStage();
    }

    // ====== ����/�ε�(����ں� Ű) ======
    void SaveProgress()
    {
        // ������ �ִ� �������� ����
        int reached = Mathf.Max(PlayerPrefs.GetInt($"{_userKeyPrefix}_reachedStage", 0), stats.stage);
        PlayerPrefs.SetInt($"{_userKeyPrefix}_reachedStage", reached);

        PlayerPrefs.SetInt($"{_userKeyPrefix}_stage", stats.stage);
        PlayerPrefs.SetInt($"{_userKeyPrefix}_exp", stats.exp);
        PlayerPrefs.SetInt($"{_userKeyPrefix}_maxExp", stats.maxExp);
        PlayerPrefs.SetInt($"{_userKeyPrefix}_hp", stats.hp);
        PlayerPrefs.SetInt($"{_userKeyPrefix}_mp", stats.mp);
        PlayerPrefs.SetInt($"{_userKeyPrefix}_gold", gold);
        PlayerPrefs.Save();
    }

    void LoadProgress()
    {
        // ����� ���� ������ �⺻�� ����
        stats.stage = PlayerPrefs.GetInt($"{_userKeyPrefix}_stage", stats.stage);
        stats.exp = PlayerPrefs.GetInt($"{_userKeyPrefix}_exp", stats.exp);
        stats.maxExp = PlayerPrefs.GetInt($"{_userKeyPrefix}_maxExp", stats.maxExp);
        stats.hp = PlayerPrefs.GetInt($"{_userKeyPrefix}_hp", stats.hp);
        stats.mp = PlayerPrefs.GetInt($"{_userKeyPrefix}_mp", stats.mp);
        gold = PlayerPrefs.GetInt($"{_userKeyPrefix}_gold", gold);

        // SO�� ������ �⺻ġ ������(�ִ�ġ�� �پ��ٸ� ���簪�� Ŭ����)
        ApplyPlayerBase();
        stats.hp = Mathf.Clamp(stats.hp, 0, stats.maxHp);
        stats.mp = Mathf.Clamp(stats.mp, 0, stats.maxMp);

        // ���� ���������� �䱸 ����ġ/���� ��Ģ �ݿ�
        ClampStageAndExp();
    }

    // ====== �� ��ü (���� ����) ======
    void ReplaceEnemiesForCurrentStage()
    {
        var st = CurStage;
        if (st && st.enemyPrefab != null)
        {
            // SpawnManager�� ������Ʈ�� �ִٸ� ȣ��, ��� ����
            try { SpawnManager.ReplaceEnemies(st.enemyPrefab); }
            catch { /* ������ ���� */ }
        }
    }
}
