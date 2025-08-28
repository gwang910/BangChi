using UnityEngine;

public interface IDamageable { void TakeDamage(int dmg); bool IsDead { get; } }

public class Health : MonoBehaviour, IDamageable
{
    public int maxHP = 100;
    public int currentHP;
    public System.Action OnDead;
    public System.Action OnChanged;

    void Awake() { currentHP = maxHP; }

    public void TakeDamage(int dmg)
    {
        if (IsDead) return;
        currentHP = Mathf.Max(0, currentHP - Mathf.Max(0, dmg));
        OnChanged?.Invoke();
        if (currentHP <= 0) { OnDead?.Invoke(); }
    }
    public bool IsDead => currentHP <= 0;
    public void Heal(int v) { if (IsDead) return; currentHP = Mathf.Min(maxHP, currentHP + v); OnChanged?.Invoke(); }
}
