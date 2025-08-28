using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    [Header("Runtime")]
    public string userId = "Guest";
    public PlayerStats stats = new PlayerStats();

    public event Action OnStatsChanged;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        // �α��ο��� ������ ID ����
        userId = PlayerPrefs.GetString("userId", "Guest");
        // �ʱ� ���� ��ε�ĳ��Ʈ
        OnStatsChanged?.Invoke();
    }

    // ��: ��ġ�� ���� ������ �� �޼���� ȣ��
    public void AddExp(int amount)
    {
        stats.exp += amount;
        while (stats.exp >= stats.maxExp) { stats.exp -= stats.maxExp; stats.stage++; }
        OnStatsChanged?.Invoke();
    }
    public void TakeDamage(int dmg)
    {
        stats.hp = Mathf.Max(0, stats.hp - dmg);
        OnStatsChanged?.Invoke();
    }
    public void Heal(int amt)
    {
        stats.hp = Mathf.Min(stats.maxHp, stats.hp + amt);
        OnStatsChanged?.Invoke();
    }
}
