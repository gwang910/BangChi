using System.Collections;
using UnityEngine;

public enum PlayerState { Idle, Seek, Attack }

[RequireComponent(typeof(Health))]
public class PlayerFSM : MonoBehaviour, IDamageable
{
    [Header("Targeting")]
    public float seekRange = 15f;
    public float attackRange = 2.2f;
    public float moveSpeed = 4f;
    public LayerMask enemyMask;

    [Header("Combat")]
    public int damage = 15;
    public float attackInterval = 0.8f;

    Transform _t;
    Health _hp;
    Coroutine _attackLoop;
    PlayerState _state;
    Transform _target;

    void Awake() { _t = transform; _hp = GetComponent<Health>(); }

    void OnEnable() { SetState(PlayerState.Idle); }

    void Update()
    {
        if (_state == PlayerState.Idle) { FindTarget(); }
        else if (_state == PlayerState.Seek) { SeekUpdate(); }
        else if (_state == PlayerState.Attack) { AttackUpdate(); }
    }

    void SetState(PlayerState s)
    {
        if (_state == s) return;
        if (_state == PlayerState.Attack && _attackLoop != null) { StopCoroutine(_attackLoop); _attackLoop = null; }
        _state = s;
        if (_state == PlayerState.Attack && _attackLoop == null) _attackLoop = StartCoroutine(AttackLoop());
    }

    void FindTarget()
    {
        Collider[] cols = Physics.OverlapSphere(_t.position, seekRange, enemyMask);
        float best = float.MaxValue; Transform bestT = null;
        foreach (var c in cols)
        {
            var h = c.GetComponentInParent<Health>();
            if (h != null && !h.IsDead)
            {
                float d = Vector3.Distance(_t.position, c.transform.position);
                if (d < best) { best = d; bestT = c.transform; }
            }
        }
        _target = bestT;
        if (_target) SetState(best <= attackRange ? PlayerState.Attack : PlayerState.Seek);
    }

    void SeekUpdate()
    {
        if (!_target) { SetState(PlayerState.Idle); return; }
        float d = Vector3.Distance(_t.position, _target.position);
        if (d > seekRange * 1.2f || _target.GetComponentInParent<Health>()?.IsDead == true)
        { _target = null; SetState(PlayerState.Idle); return; }

        if (d > attackRange)
        {
            var dir = (_target.position - _t.position);
            dir.y = 0;
            _t.position += dir.normalized * moveSpeed * Time.deltaTime;
            _t.forward = Vector3.Slerp(_t.forward, dir.normalized, 10f * Time.deltaTime);
        }
        else SetState(PlayerState.Attack);
    }

    void AttackUpdate()
    {
        if (!_target) { SetState(PlayerState.Idle); return; }
        float d = Vector3.Distance(_t.position, _target.position);
        if (d > attackRange * 1.2f) { SetState(PlayerState.Seek); return; }
        // 바라보기만 유지. 실제 때리기는 코루틴에서 주기적으로 수행
        var dir = (_target.position - _t.position); dir.y = 0;
        if (dir.sqrMagnitude > 0.01f) _t.forward = Vector3.Slerp(_t.forward, dir.normalized, 10f * Time.deltaTime);
    }

    IEnumerator AttackLoop()
    {
        var wait = new WaitForSeconds(attackInterval);
        while (_state == PlayerState.Attack)
        {
            var dmg = _target ? _target.GetComponentInParent<IDamageable>() : null;
            if (dmg != null && !dmg.IsDead) dmg.TakeDamage(damage);
            yield return wait;
        }
    }

    // IDamageable (플레이어도 맞을 수 있음)
    public void TakeDamage(int dmg) { _hp.TakeDamage(dmg); }
    public bool IsDead => _hp.IsDead;
}
