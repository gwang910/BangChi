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
    public CharacterStatsSO playerBase;     // 기본 스탯 SO
    public StageConfigSO[] stages;          // 스테이지 설정 리스트

    [Header("Runtime")]
    public string userId = "Guest";
    public PlayerStats stats = new PlayerStats();

    public int gold = 0;

    public event Action OnStatsChanged;

    string _userKeyPrefix => $"{userId}";   // 사용자별 키 prefix

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        // 로그인에서 저장한 ID 복구
        userId = PlayerPrefs.GetString("userId", "Guest");

        if ((stages == null || stages.Length == 0))
        {
            var loaded = Resources.LoadAll<StageConfigSO>("Stages");
            if (loaded != null && loaded.Length > 0)
                stages = loaded.OrderBy(s => s.stageNumber).ToArray();
        }

        ApplyPlayerBase();   // SO가 있으면 기본 능력치 반영
        LoadProgress();      // 사용자별 진행 로드
        ClampStageAndExp();  // 로드값을 현재 스테이지 요구치에 맞춤

        // 초기 상태 브로드캐스트
        OnStatsChanged?.Invoke();

        // 시작 시 현재 스테이지의 적으로 교체(있으면)
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

    // ====== 현재 스테이지 설정 조회 ======
    public StageConfigSO CurStage
    {
        get
        {
            if (stages == null || stages.Length == 0) return null;
            int idx = Mathf.Clamp(stats.stage, 0, stages.Length - 1);
            return stages[idx];
        }
    }

    // 현재 스테이지 요구치/적 반영
    void ClampStageAndExp()
    {
        var st = CurStage;
        if (st != null) stats.maxExp = Mathf.Max(1, st.expToNext);
        stats.exp = Mathf.Clamp(stats.exp, 0, stats.maxExp);
    }

    // ====== 전투/경험치 ======
    // 적 처치시 호출하면 경험치/골드 반영 후 필요하면 스테이지 상승
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
            // 스테이지 상승 시 새 요구치 반영
            var next = CurStage;
            stats.maxExp = next ? Mathf.Max(1, next.expToNext) : stats.maxExp;
        }

        SaveProgress();
        OnStatsChanged?.Invoke();

        // 더 강한 적으로 교체 (SpawnManager가 있으면)
        ReplaceEnemiesForCurrentStage();
    }

    // 수치가 변할 때마다 이 메서드들 호출
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
        // 플레이어가 도달한 최대 스테이지까지 허용
        int reached = PlayerPrefs.GetInt($"{_userKeyPrefix}_reachedStage", 0);
        int maxAllowed = Mathf.Max(reached, stats.stage);
        stats.stage = Mathf.Clamp(targetStage, 0, maxAllowed);

        // 이동 시 경험치는 초기화
        stats.exp = 0;
        ClampStageAndExp();

        SaveProgress();
        OnStatsChanged?.Invoke();
        ReplaceEnemiesForCurrentStage();
    }

    // ====== 저장/로드(사용자별 키) ======
    void SaveProgress()
    {
        // 도달한 최대 스테이지 갱신
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
        // 저장된 값이 없으면 기본값 유지
        stats.stage = PlayerPrefs.GetInt($"{_userKeyPrefix}_stage", stats.stage);
        stats.exp = PlayerPrefs.GetInt($"{_userKeyPrefix}_exp", stats.exp);
        stats.maxExp = PlayerPrefs.GetInt($"{_userKeyPrefix}_maxExp", stats.maxExp);
        stats.hp = PlayerPrefs.GetInt($"{_userKeyPrefix}_hp", stats.hp);
        stats.mp = PlayerPrefs.GetInt($"{_userKeyPrefix}_mp", stats.mp);
        gold = PlayerPrefs.GetInt($"{_userKeyPrefix}_gold", gold);

        // SO가 있으면 기본치 재적용(최대치가 줄었다면 현재값도 클램프)
        ApplyPlayerBase();
        stats.hp = Mathf.Clamp(stats.hp, 0, stats.maxHp);
        stats.mp = Mathf.Clamp(stats.mp, 0, stats.maxMp);

        // 현재 스테이지의 요구 경험치/보상 규칙 반영
        ClampStageAndExp();
    }

    // ====== 적 교체 (있을 때만) ======
    void ReplaceEnemiesForCurrentStage()
    {
        var st = CurStage;
        if (st && st.enemyPrefab != null)
        {
            // SpawnManager가 프로젝트에 있다면 호출, 없어도 안전
            try { SpawnManager.ReplaceEnemies(st.enemyPrefab); }
            catch { /* 없으면 무시 */ }
        }
    }
}
