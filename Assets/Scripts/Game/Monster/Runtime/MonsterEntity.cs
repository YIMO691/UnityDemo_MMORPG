using UnityEngine;
using System;

public class MonsterEntity : MonoBehaviour
{
    public int ConfigId { get; private set; }
    public int CurrentHp { get; private set; }
    public MonsterStateType CurrentState { get; private set; } = MonsterStateType.Idle;
    public Transform CurrentTarget { get; private set; }
    public bool IsDead { get; private set; }
    public event Action<MonsterEntity> OnDead;

    private MonsterConfig config;
    private MonsterNavigator navigator;
    private float attackTimer;
    private float pathTimer;
    private float pathInterval = 0.3f;
    private MonsterAnimatorDriver anim;
    private float attackHoldTimer;
    private float attackHoldDuration = 0.6f;
    private Vector3 spawnPosition;
    private float chaseCooldown = 1f;
    private float chaseCooldownTimer;
    private float attackEntryCooldown = 0.3f;
    private float attackEntryCooldownTimer;
    private bool attackFrozen;
    private Vector3 attackFreezePosition;

    public void Init(MonsterConfig cfg, Vector3 spawnPos)
    {
        config = cfg;
        ConfigId = cfg.id;
        CurrentHp = cfg.maxHp;
        spawnPosition = spawnPos;
        navigator = GetComponent<MonsterNavigator>();
        anim = GetComponent<MonsterAnimatorDriver>();
        if (anim == null) anim = gameObject.AddComponent<MonsterAnimatorDriver>();
    }

    public void SetTarget(Transform target)
    {
        CurrentTarget = target;
    }

    public void ClearTarget()
    {
        CurrentTarget = null;
    }

    public void TakeDamage(int damage)
    {
        if (IsDead) return;
        CurrentHp = Mathf.Max(0, CurrentHp - damage);
        if (CurrentHp <= 0) Die();
    }

    public void Die()
    {
        if (IsDead) return;
        IsDead = true;
        OnDead?.Invoke(this);
        if (navigator != null) navigator.StopNavigation();
        NavigationRegistry.Instance.Unregister(navigator);
        anim?.SetDead();
        Destroy(gameObject);
    }

    private void Update()
    {
        if (IsDead) return;
        if (config == null) return;
        if (navigator == null) navigator = GetComponent<MonsterNavigator>();
        if (navigator == null) return;
        if (chaseCooldownTimer > 0f) chaseCooldownTimer -= Time.deltaTime;
        if (attackEntryCooldownTimer > 0f) attackEntryCooldownTimer -= Time.deltaTime;

        if (CurrentTarget == null)
        {
            var player = PlayerLocator.Instance != null ? PlayerLocator.Instance.GetPlayerTransform() : null;
            if (player != null)
                SetTarget(player);
        }

        switch (CurrentState)
        {
            case MonsterStateType.Idle:
                TickIdle();
                break;
            case MonsterStateType.Chase:
                TickChase();
                break;
            case MonsterStateType.Attack:
                TickAttack();
                break;
            case MonsterStateType.Return:
                TickReturn();
                break;
        }
    }

    private void TickIdle()
    {
        if (CurrentTarget == null) return;
        float dist = Vector3.Distance(transform.position, CurrentTarget.position);
        anim?.SetIdle();
        if (dist <= config.detectRange)
        {
            CurrentState = MonsterStateType.Chase;
            attackEntryCooldownTimer = attackEntryCooldown;
            pathTimer = pathInterval;
        }
    }

    private void TickChase()
    {
        if (CurrentTarget == null)
        {
            CurrentState = MonsterStateType.Idle;
            return;
        }
        float dist = Vector3.Distance(transform.position, CurrentTarget.position);
        if (dist > config.detectRange)
        {
            CurrentState = MonsterStateType.Return;
            chaseCooldownTimer = chaseCooldown;
            navigator.StopNavigation();
            return;
        }
        if (dist <= config.attackRange && attackEntryCooldownTimer <= 0f)
        {
            CurrentState = MonsterStateType.Attack;
            navigator.StopNavigation();
            attackHoldTimer = attackHoldDuration;
            attackFrozen = true;
            attackFreezePosition = transform.position;
            return;
        }
        float chaseSpeed = navigator != null ? navigator.CurrentSpeed : 0f;
        anim?.SetChase(chaseSpeed);
        pathTimer += Time.deltaTime;
        if (pathTimer >= pathInterval && !string.IsNullOrEmpty(navigator.AgentId))
        {
            pathTimer = 0f;
            var req = new NavigationMoveRequest(navigator.AgentId, CurrentTarget.position, 0.2f);
            EventBus.Publish(new NavigationMoveRequestEvent(req));
        }
    }

    private void TickAttack()
    {
        if (CurrentTarget == null)
        {
            CurrentState = MonsterStateType.Idle;
            return;
        }
        // Freeze locomotion while attacking
        anim?.SetIdle();
        float dist = Vector3.Distance(transform.position, CurrentTarget.position);
        if (attackFrozen) transform.position = attackFreezePosition;
        attackTimer += Time.deltaTime;
        if (attackTimer >= config.attackInterval)
        {
            attackTimer = 0f;
            anim?.TriggerAttack();
        }
        if (attackHoldTimer > 0f)
        {
            attackHoldTimer -= Time.deltaTime;
            return;
        }
        if (dist > config.attackRange * 1.2f)
        {
            CurrentState = MonsterStateType.Chase;
            attackFrozen = false;
        }
    }

    private void TickReturn()
    {
        if (CurrentTarget != null)
        {
            float distTarget = Vector3.Distance(transform.position, CurrentTarget.position);
            if (chaseCooldownTimer <= 0f && distTarget <= config.detectRange)
            {
                CurrentState = MonsterStateType.Chase;
                pathTimer = pathInterval;
                attackEntryCooldownTimer = attackEntryCooldown;
                if (!string.IsNullOrEmpty(navigator.AgentId))
                {
                    var req = new NavigationMoveRequest(navigator.AgentId, CurrentTarget.position, 0.2f);
                    EventBus.Publish(new NavigationMoveRequestEvent(req));
                }
                return;
            }
        }
        float distHome = Vector3.Distance(transform.position, spawnPosition);
        if (distHome <= 0.5f)
        {
            CurrentState = MonsterStateType.Idle;
            ClearTarget();
            return;
        }
        pathTimer += Time.deltaTime;
        if (pathTimer >= pathInterval && !string.IsNullOrEmpty(navigator.AgentId))
        {
            pathTimer = 0f;
            var req = new NavigationMoveRequest(navigator.AgentId, spawnPosition, 0.1f);
            EventBus.Publish(new NavigationMoveRequestEvent(req));
        }
    }

    public float GetMoveSpeed()
    {
        return config != null ? config.moveSpeed : 3f;
    }

    // Animation event receivers
    public void OnBornOver() { }
    public void OnAttackEvent()
    {
        attackHoldTimer = 0f;
    }
}
