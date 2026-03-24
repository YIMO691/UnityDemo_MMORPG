using UnityEngine;

public class MonsterBrain : MonoBehaviour
{
    [SerializeField] private float pathInterval = 0.3f;
    [SerializeField] private float chaseCooldown = 1f;
    [SerializeField] private float attackEntryCooldown = 0.3f;
    [SerializeField] private float attackFailSafeTimeout = 2f;

    private MonsterEntity entity;
    private MonsterNavigator navigator;
    private MonsterAnimatorDriver anim;

    private float attackTimer;
    private float pathTimer;
    private float chaseCooldownTimer;
    private float attackEntryCooldownTimer;
    private bool attackFrozen;
    private float attackFailSafeTimer;

    private void Awake()
    {
        entity = GetComponent<MonsterEntity>();
        navigator = GetComponent<MonsterNavigator>();
        anim = GetComponent<MonsterAnimatorDriver>();
    }

    private void Update()
    {
        if (entity == null || entity.IsDead) return;
        if (entity.Config == null) return;
        if (navigator == null) navigator = GetComponent<MonsterNavigator>();
        if (anim == null) anim = GetComponent<MonsterAnimatorDriver>();
        if (navigator == null) return;

        if (chaseCooldownTimer > 0f) chaseCooldownTimer -= Time.deltaTime;
        if (attackEntryCooldownTimer > 0f) attackEntryCooldownTimer -= Time.deltaTime;

        EnsureTarget();

        switch (entity.CurrentState)
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

    private void EnsureTarget()
    {
        if (entity.CurrentTarget != null) return;

        var player = PlayerLocator.Instance != null
            ? PlayerLocator.Instance.GetPlayerTransform()
            : null;

        if (player != null)
            entity.SetTarget(player);
    }

    private void TickIdle()
    {
        anim?.SetIdle();

        if (entity.CurrentTarget == null) return;

        float dist = Vector3.Distance(transform.position, entity.CurrentTarget.position);
        if (dist <= entity.Config.detectRange)
        {
            entity.SetState(MonsterStateType.Chase);
            attackEntryCooldownTimer = attackEntryCooldown;
            pathTimer = pathInterval;
        }
    }

    private void TickChase()
    {
        if (entity.CurrentTarget == null)
        {
            entity.SetState(MonsterStateType.Idle);
            navigator.StopNavigation();
            return;
        }

        float dist = Vector3.Distance(transform.position, entity.CurrentTarget.position);

        if (dist > entity.Config.detectRange)
        {
            entity.SetState(MonsterStateType.Return);
            chaseCooldownTimer = chaseCooldown;
            navigator.StopNavigation();
            return;
        }

        if (dist <= entity.Config.attackRange && attackEntryCooldownTimer <= 0f)
        {
            entity.SetState(MonsterStateType.Attack);
            navigator.StopNavigation();
            attackFrozen = true;
            attackFailSafeTimer = 0f;
            attackTimer = 0f;
            return;
        }

        anim?.SetChase(navigator.CurrentSpeed);

        pathTimer += Time.deltaTime;
        if (pathTimer >= pathInterval)
        {
            pathTimer = 0f;
            navigator.MoveTo(entity.CurrentTarget.position, 0.2f);
        }
    }

    private void TickAttack()
    {
        if (entity.CurrentTarget == null)
        {
            entity.SetState(MonsterStateType.Idle);
            return;
        }

        anim?.SetIdle();

        float dist = Vector3.Distance(transform.position, entity.CurrentTarget.position);

        attackTimer += Time.deltaTime;
        attackFailSafeTimer += Time.deltaTime;

        if (attackFailSafeTimer >= attackFailSafeTimeout)
            attackFrozen = false;

        if (attackTimer >= entity.Config.attackInterval)
        {
            attackTimer = 0f;
            anim?.TriggerAttack();
        }

        if (!attackFrozen && dist > entity.Config.attackRange * 1.2f)
        {
            entity.SetState(MonsterStateType.Chase);
            pathTimer = pathInterval;
        }
    }

    private void TickReturn()
    {
        if (entity.CurrentTarget != null)
        {
            float distTarget = Vector3.Distance(transform.position, entity.CurrentTarget.position);
            if (chaseCooldownTimer <= 0f && distTarget <= entity.Config.detectRange)
            {
                entity.SetState(MonsterStateType.Chase);
                pathTimer = pathInterval;
                attackEntryCooldownTimer = attackEntryCooldown;
                navigator.MoveTo(entity.CurrentTarget.position, 0.2f);
                return;
            }
        }

        float distHome = Vector3.Distance(transform.position, entity.SpawnPosition);
        if (distHome <= 0.5f)
        {
            navigator.StopNavigation();
            entity.ClearTarget();
            entity.SetState(MonsterStateType.Idle);
            anim?.SetIdle();
            return;
        }

        anim?.SetChase(navigator.CurrentSpeed);

        pathTimer += Time.deltaTime;
        if (pathTimer >= pathInterval)
        {
            pathTimer = 0f;
            navigator.MoveTo(entity.SpawnPosition, 0.1f);
        }
    }

    public void OnBornOver() { }
    public void OnAttackEvent() { }
    public void OnAttackOver()
    {
        attackFrozen = false;
        attackFailSafeTimer = 0f;
    }
}
