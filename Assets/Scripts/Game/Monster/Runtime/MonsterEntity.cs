using UnityEngine;
using System;

public class MonsterEntity : MonoBehaviour, IDamageReceiver, ICombatSource, ILootProvider
{
    public int ConfigId { get; private set; }
    public MonsterConfig Config { get; private set; }
    public int CurrentHp { get; private set; }
    public MonsterStateType CurrentState { get; private set; } = MonsterStateType.Idle;
    public Transform CurrentTarget { get; private set; }
    public bool IsDead { get; private set; }
    public string RuntimeId { get; private set; }
    public string SpawnPointId { get; private set; }
    public Vector3 SpawnPosition { get; private set; }

    public event Action<MonsterEntity> OnDead;

    private MonsterNavigator navigator;
    private MonsterAnimatorDriver anim;

    public Transform ActorTransform => transform;
    public string DisplayName => Config != null ? Config.name : "Monster";
    public int FactionId => 2;

    public int MaxHp => Config != null ? Config.maxHp : 0;
    public float MoveSpeed => Config != null ? Config.moveSpeed : 0f;

    // 战斗属性直接走 MonsterConfig
    public int Attack => Config != null ? Config.attack : 0;
    public int Defense => Config != null ? Config.defense : 0;

    public float CritRate => Config != null ? Config.critRate : 0f;
    public float CritDamage => Config != null ? Config.critDamage : 1.5f;
    public float HitRate => Config != null ? Config.hitRate : 1f;
    public float DodgeRate => Config != null ? Config.dodgeRate : 0f;

    public void Init(MonsterConfig cfg, Vector3 spawnPos)
    {
        if (cfg == null)
        {
            Debug.LogError("[MonsterEntity] Init failed: cfg is null");
            return;
        }

        Config = cfg;
        ConfigId = cfg.id;
        SpawnPosition = spawnPos;

        CurrentHp = Mathf.Max(1, cfg.maxHp);
        IsDead = false;
        CurrentState = MonsterStateType.Idle;
        CurrentTarget = null;

        navigator = GetComponent<MonsterNavigator>();
        anim = GetComponent<MonsterAnimatorDriver>();
        if (anim == null)
            anim = gameObject.AddComponent<MonsterAnimatorDriver>();

        Debug.Log($"[MonsterEntity] Init success, name={DisplayName}, hp={CurrentHp}/{MaxHp}, atk={Attack}, def={Defense}");
    }

    public void SetIdentity(string runtimeId, string spawnPointId = null)
    {
        RuntimeId = runtimeId;
        SpawnPointId = spawnPointId;
    }

    public void SetTarget(Transform target)
    {
        if (IsDead) return;
        CurrentTarget = target;
    }

    public void ClearTarget()
    {
        CurrentTarget = null;
    }

    public void SetState(MonsterStateType state)
    {
        if (IsDead)
        {
            CurrentState = MonsterStateType.Idle;
            return;
        }

        CurrentState = state;
    }

    public float GetMoveSpeed()
    {
        return Config != null ? Config.moveSpeed : 3f;
    }

    public MonsterNavigator GetNavigator()
    {
        if (navigator == null)
            navigator = GetComponent<MonsterNavigator>();
        return navigator;
    }

    public MonsterAnimatorDriver GetAnimatorDriver()
    {
        if (anim == null)
            anim = GetComponent<MonsterAnimatorDriver>();
        return anim;
    }

    public void TakeDamage(int damage)
    {
        if (IsDead) return;

        int finalDamage = Mathf.Max(0, damage);
        if (finalDamage <= 0) return;

        int oldHp = CurrentHp;
        CurrentHp = Mathf.Max(0, CurrentHp - finalDamage);

        Debug.Log($"[MonsterEntity] TakeDamage name={DisplayName}, damage={finalDamage}, hp={oldHp}->{CurrentHp}");

        if (CurrentHp <= 0)
        {
            Die();
        }
    }

    public void ReceiveDamage(int damage)
    {
        TakeDamage(damage);
    }

    public void SetSpawnPosition(Vector3 pos)
    {
        SpawnPosition = pos;
    }

    public void Die()
    {
        if (IsDead) return;

        IsDead = true;
        CurrentHp = 0;
        CurrentState = MonsterStateType.Idle;
        CurrentTarget = null;

        Debug.Log($"[MonsterEntity] Dead, name={DisplayName}, runtimeId={RuntimeId}");

        OnDead?.Invoke(this);
    }

    public void DropLoot(ICombatSource killer)
    {
        LootRuntimeService.HandleMonsterLoot(this, killer);
    }

    public MonsterSaveData BuildSaveData()
    {
        return new MonsterSaveData
        {
            runtimeId = RuntimeId,
            configId = ConfigId,
            sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name,
            posX = transform.position.x,
            posY = transform.position.y,
            posZ = transform.position.z,
            rotY = transform.eulerAngles.y,
            currentHp = CurrentHp,
            isDead = IsDead,
            spawnPointId = SpawnPointId
        };
    }

    public void ApplySaveData(MonsterSaveData data)
    {
        if (data == null)
        {
            Debug.LogWarning("[MonsterEntity] ApplySaveData skipped: data is null");
            return;
        }

        transform.position = new Vector3(data.posX, data.posY, data.posZ);
        transform.rotation = Quaternion.Euler(0f, data.rotY, 0f);

        CurrentState = MonsterStateType.Idle;
        CurrentTarget = null;

        int maxHp = MaxHp > 0 ? MaxHp : (Config != null ? Config.maxHp : 1);
        CurrentHp = Mathf.Clamp(data.currentHp, 0, maxHp);
        IsDead = data.isDead || CurrentHp <= 0;

        if (IsDead)
        {
            CurrentHp = 0;
        }

        Debug.Log($"[MonsterEntity] ApplySaveData name={DisplayName}, hp={CurrentHp}/{maxHp}, isDead={IsDead}");
    }
}
