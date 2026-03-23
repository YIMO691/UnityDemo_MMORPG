using UnityEngine;

public class MonsterEntity : MonoBehaviour
{
    public int ConfigId { get; private set; }
    public int CurrentHp { get; private set; }
    public MonsterStateType CurrentState { get; private set; } = MonsterStateType.Idle;
    public Transform CurrentTarget { get; private set; }
    public bool IsDead { get; private set; }

    private MonsterConfig config;
    private MonsterNavigator navigator;
    private float attackTimer;

    public void Init(MonsterConfig cfg)
    {
        config = cfg;
        ConfigId = cfg.id;
        CurrentHp = cfg.maxHp;
        navigator = GetComponent<MonsterNavigator>();
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
        if (navigator != null) navigator.StopNavigation();
        NavigationRegistry.Instance.Unregister(navigator);
        Destroy(gameObject);
    }

    private void Update()
    {
        if (IsDead) return;
        if (navigator == null) navigator = GetComponent<MonsterNavigator>();
        if (navigator == null) return;
        Transform player = PlayerLocator.Instance.GetPlayerTransform();
        if (player != null)
        {
            float dist = Vector3.Distance(transform.position, player.position);
            if (CurrentState == MonsterStateType.Idle && dist <= config.detectRange)
            {
                SetTarget(player);
                CurrentState = MonsterStateType.Chase;
                if (!string.IsNullOrEmpty(navigator.AgentId))
                {
                    var req = new NavigationMoveRequest(navigator.AgentId, player.position, config.attackRange);
                    EventBus.Publish(new NavigationMoveRequestEvent(req));
                }
            }
            else if (CurrentState == MonsterStateType.Chase)
            {
                if (dist > config.detectRange * 1.5f)
                {
                    CurrentState = MonsterStateType.Idle;
                    if (navigator != null) navigator.StopNavigation();
                    ClearTarget();
                }
                else if (dist <= config.attackRange)
                {
                    CurrentState = MonsterStateType.Attack;
                    if (navigator != null) navigator.StopNavigation();
                }
            }
            else if (CurrentState == MonsterStateType.Attack)
            {
                attackTimer += Time.deltaTime;
                if (attackTimer >= config.attackInterval)
                {
                    attackTimer = 0f;
                }
                if (dist > config.attackRange * 1.2f)
                {
                    CurrentState = MonsterStateType.Chase;
                    if (!string.IsNullOrEmpty(navigator.AgentId))
                    {
                        var req = new NavigationMoveRequest(navigator.AgentId, player.position, config.attackRange);
                        EventBus.Publish(new NavigationMoveRequestEvent(req));
                    }
                }
            }
        }
    }

    public float GetMoveSpeed()
    {
        return config != null ? config.moveSpeed : 3f;
    }
}
