using UnityEngine;
using System;

public class MonsterEntity : MonoBehaviour
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

    public void Init(MonsterConfig cfg, Vector3 spawnPos)
    {
        Config = cfg;
        ConfigId = cfg.id;
        CurrentHp = cfg.maxHp;
        SpawnPosition = spawnPos;
        IsDead = false;
        CurrentState = MonsterStateType.Idle;

        navigator = GetComponent<MonsterNavigator>();
        anim = GetComponent<MonsterAnimatorDriver>();
        if (anim == null) anim = gameObject.AddComponent<MonsterAnimatorDriver>();
    }

    public void SetIdentity(string runtimeId, string spawnPointId = null)
    {
        RuntimeId = runtimeId;
        SpawnPointId = spawnPointId;
    }

    public void SetTarget(Transform target)
    {
        CurrentTarget = target;
    }

    public void ClearTarget()
    {
        CurrentTarget = null;
    }

    public void SetState(MonsterStateType state)
    {
        CurrentState = state;
    }

    public float GetMoveSpeed()
    {
        return Config != null ? Config.moveSpeed : 3f;
    }

    public MonsterNavigator GetNavigator()
    {
        if (navigator == null) navigator = GetComponent<MonsterNavigator>();
        return navigator;
    }

    public MonsterAnimatorDriver GetAnimatorDriver()
    {
        if (anim == null) anim = GetComponent<MonsterAnimatorDriver>();
        return anim;
    }

    public void TakeDamage(int damage)
    {
        if (IsDead) return;
        CurrentHp = Mathf.Max(0, CurrentHp - damage);
        if (CurrentHp <= 0) Die();
    }

    public void SetSpawnPosition(Vector3 pos)
    {
        SpawnPosition = pos;
    }

    public void Die()
    {
        if (IsDead) return;

        IsDead = true;
        CurrentState = MonsterStateType.Idle;

        if (navigator == null) navigator = GetComponent<MonsterNavigator>();
        if (navigator != null)
        {
            navigator.StopNavigation();
            NavigationRegistry.Instance.Unregister(navigator);
        }

        MonsterRuntimeRegistry.Instance.Unregister(this);

        anim = GetComponent<MonsterAnimatorDriver>();
        anim?.SetDead();

        OnDead?.Invoke(this);

        Destroy(gameObject);
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
        transform.position = new Vector3(data.posX, data.posY, data.posZ);
        transform.rotation = Quaternion.Euler(0f, data.rotY, 0f);
        CurrentHp = data.currentHp;
        CurrentState = MonsterStateType.Idle;

        if (data.isDead)
        {
            Die();
        }
    }
}
