using UnityEngine;

[RequireComponent(typeof(MonsterEntity))]
[RequireComponent(typeof(MonsterLocomotionExecutor))]
public class MonsterBrain : MonoBehaviour
{
    [SerializeField] private float pathInterval = 0.3f;
    [SerializeField] private float chaseCooldown = 1f;
    [SerializeField] private float attackEntryCooldown = 0.3f;
    [SerializeField] private float attackFailSafeTimeout = 2f;

    // 这里作为附加伤害。当前推荐先设为 0，
    // 让 BattleDamageService 主要通过 Attack - Defense 去结算。
    [SerializeField] private int bonusRawDamage = 0;

    private MonsterEntity entity;
    private MonsterLocomotionExecutor executor;

    private float attackTimer;
    private float pathTimer;
    private float chaseCooldownTimer;
    private float attackEntryCooldownTimer;
    private bool attackFrozen;
    private float attackFailSafeTimer;

    private void Awake()
    {
        entity = GetComponent<MonsterEntity>();
        executor = GetComponent<MonsterLocomotionExecutor>();
    }

    private void Update()
    {
        if (entity == null || entity.IsDead) return;
        if (entity.Config == null) return;

        if (chaseCooldownTimer > 0f)
            chaseCooldownTimer -= Time.deltaTime;

        if (attackEntryCooldownTimer > 0f)
            attackEntryCooldownTimer -= Time.deltaTime;

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
        {
            entity.SetTarget(player);
        }
    }

    private void TickIdle()
    {
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
            executor.StopAll();
            return;
        }

        float dist = Vector3.Distance(transform.position, entity.CurrentTarget.position);

        if (dist > entity.Config.detectRange)
        {
            entity.SetState(MonsterStateType.Return);
            chaseCooldownTimer = chaseCooldown;
            executor.StopAll();
            return;
        }

        if (dist <= entity.Config.attackRange && attackEntryCooldownTimer <= 0f)
        {
            entity.SetState(MonsterStateType.Attack);
            executor.StopAll();
            attackFrozen = true;
            attackFailSafeTimer = 0f;
            attackTimer = 0f;
            return;
        }

        pathTimer += Time.deltaTime;
        if (pathTimer >= pathInterval)
        {
            pathTimer = 0f;

            Vector3 dir = (entity.CurrentTarget.position - transform.position).normalized;

            executor.Execute(
                new ActorControlCommand
                {
                    moveDirection = dir,
                    lookDirection = dir,
                    sprint = false,
                    attack = false,
                    stop = false
                },
                entity.CurrentTarget.position,
                0.2f);
        }
    }

    private void TickAttack()
    {
        if (entity.CurrentTarget == null)
        {
            entity.SetState(MonsterStateType.Idle);
            executor.StopAll();
            return;
        }

        float dist = Vector3.Distance(transform.position, entity.CurrentTarget.position);

        attackTimer += Time.deltaTime;
        attackFailSafeTimer += Time.deltaTime;

        if (attackFailSafeTimer >= attackFailSafeTimeout)
            attackFrozen = false;

        if (attackTimer >= entity.Config.attackInterval)
        {
            attackTimer = 0f;

            Vector3 dir = (entity.CurrentTarget.position - transform.position).normalized;

            executor.Execute(new ActorControlCommand
            {
                moveDirection = Vector3.zero,
                lookDirection = dir,
                sprint = false,
                attack = true,
                stop = true
            });
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
                return;
            }
        }

        float distHome = Vector3.Distance(transform.position, entity.SpawnPosition);
        if (distHome <= 0.5f)
        {
            executor.StopAll();
            entity.ClearTarget();
            entity.SetState(MonsterStateType.Idle);
            return;
        }

        pathTimer += Time.deltaTime;
        if (pathTimer >= pathInterval)
        {
            pathTimer = 0f;

            Vector3 dir = (entity.SpawnPosition - transform.position).normalized;

            executor.Execute(
                new ActorControlCommand
                {
                    moveDirection = dir,
                    lookDirection = dir,
                    sprint = false,
                    attack = false,
                    stop = false
                },
                entity.SpawnPosition,
                0.1f);
        }
    }

    public void OnBornOver()
    {
    }

    public void OnAttackEvent()
    {
        Debug.Log("[MonsterBrain] OnAttackEvent enter");

        if (entity == null)
        {
            Debug.LogWarning("[MonsterBrain] entity is null");
            return;
        }

        if (entity.IsDead)
        {
            Debug.Log("[MonsterBrain] entity is dead");
            return;
        }

        if (entity.CurrentTarget == null)
        {
            Debug.Log("[MonsterBrain] currentTarget is null");
            return;
        }

        ICombatSource attacker = GetComponent<ICombatSource>();
        IDamageReceiver target = CombatTargetResolver.ResolveDamageReceiver(entity.CurrentTarget);

        Debug.Log($"[MonsterBrain] currentTarget={entity.CurrentTarget.name}");
        Debug.Log($"[MonsterBrain] attacker null = {attacker == null}");
        Debug.Log($"[MonsterBrain] target null = {target == null}");
        Debug.Log($"[MonsterBrain] target.IsDead = {target?.IsDead}");
        Debug.Log($"[MonsterBrain] target type = {target?.GetType().Name}");
        Debug.Log($"[MonsterBrain] attackerFaction = {(attacker as IFactionProvider)?.FactionId}, targetFaction = {(target as IFactionProvider)?.FactionId}");

        if (!CombatTargetResolver.IsValidHostileTarget(attacker, target))
        {
            Debug.Log("[MonsterBrain] target is not a valid hostile target");
            return;
        }

        float dist = Vector3.Distance(transform.position, entity.CurrentTarget.position);
        if (dist > entity.Config.attackRange * 1.3f)
        {
            Debug.Log($"[MonsterBrain] target out of attack range, dist={dist}");
            return;
        }

        Vector3 hitPos = entity.CurrentTarget.position;

        // 当前推荐：附加伤害先保持 0 或很小值，
        // 主体伤害由 BattleDamageService 内部的 Attack / Defense 结算。
        int rawDamage = Mathf.Max(0, bonusRawDamage);

        var request = CombatRequestFactory.CreateBasicDamage(
            attacker,
            target,
            rawDamage,
            hitPos,
            DamageSourceType.NormalAttack);

        BattleDamageService.Instance.ApplyDamage(request);

        Debug.Log($"[MonsterBrain] damage applied: raw={rawDamage}, monsterAttack={entity.Attack}");
    }

    public void OnAttackOver()
    {
        attackFrozen = false;
        attackFailSafeTimer = 0f;
    }
}
