using System.Collections;
using UnityEngine;

public enum EnemyState { Idle, Chase, Attack, Dead }

[RequireComponent(typeof(Health))]
public class EnemyFSM : MonoBehaviour, IDamageable
{
    public float detectRange = 12f;
    public float attackRange = 2.2f;
    public float moveSpeed = 3.2f;
    public int damage = 7;
    public float attackInterval = 1.0f;

    Transform _t; Transform _player;
    Health _hp;
    EnemyState _state;
    Coroutine _attack;

    void Awake() { _t = transform; _hp = GetComponent<Health>(); }
    void Start() { _player = GameObject.FindGameObjectWithTag("Player").transform; SetState(EnemyState.Idle); }
    void OnEnable() { _hp.OnDead += OnDead; }

    void Update()
    {
        if (_state == EnemyState.Dead) return;
        float d = Vector3.Distance(_t.position, _player.position);
        if (_state == EnemyState.Idle)
        {
            if (d <= detectRange) SetState(EnemyState.Chase);
        }
        else if (_state == EnemyState.Chase)
        {
            if (d > detectRange * 1.3f) SetState(EnemyState.Idle);
            else if (d <= attackRange) SetState(EnemyState.Attack);
            else MoveToPlayer();
        }
        else if (_state == EnemyState.Attack)
        {
            if (d > attackRange * 1.2f) SetState(EnemyState.Chase);
            else FacePlayer();
        }
    }

    void MoveToPlayer()
    {
        var dir = (_player.position - _t.position); dir.y = 0;
        _t.position += dir.normalized * moveSpeed * Time.deltaTime;
        _t.forward = Vector3.Slerp(_t.forward, dir.normalized, 10f * Time.deltaTime);
    }
    void FacePlayer() { var dir = (_player.position - _t.position); dir.y = 0; if (dir.sqrMagnitude > 0.01f) _t.forward = Vector3.Slerp(_t.forward, dir.normalized, 10f * Time.deltaTime); }

    void SetState(EnemyState s)
    {
        if (_state == s) return;
        if (_state == EnemyState.Attack && _attack != null) { StopCoroutine(_attack); _attack = null; }
        _state = s;
        if (_state == EnemyState.Attack) _attack = StartCoroutine(AttackLoop());
    }
    IEnumerator AttackLoop()
    {
        var wait = new WaitForSeconds(attackInterval);
        while (_state == EnemyState.Attack)
        {
            var dmg = _player.GetComponent<IDamageable>();
            if (dmg != null && !dmg.IsDead) dmg.TakeDamage(damage);
            yield return wait;
        }
    }
    void OnDead() { SetState(EnemyState.Dead); StopAllCoroutines(); enabled = false; }
    public void TakeDamage(int dmg) { _hp.TakeDamage(dmg); }
    public bool IsDead => _hp.IsDead;
}
