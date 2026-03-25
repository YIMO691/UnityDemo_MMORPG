using UnityEngine;

public class PlayerEntity : MonoBehaviour, IDamageReceiver, ICombatSource
{
    public string RuntimeId { get; private set; }
    public PlayerData Data { get; private set; }

    public Transform ActorTransform => transform;

    public string DisplayName
    {
        get
        {
            return Data != null && Data.baseData != null
                ? Data.baseData.roleName
                : "Player";
        }
    }

    public bool IsDead
    {
        get
        {
            return Data != null &&
                   Data.runtimeData != null &&
                   Data.runtimeData.isDead;
        }
    }

    public int FactionId => 1;

    public int MaxHp
    {
        get
        {
            return Data != null && Data.attributeData != null
                ? Data.attributeData.maxHp
                : 0;
        }
    }

    public int CurrentHp
    {
        get
        {
            return Data != null && Data.runtimeData != null
                ? Data.runtimeData.currentHp
                : 0;
        }
    }

    public float MoveSpeed
    {
        get
        {
            return Data != null && Data.attributeData != null
                ? Data.attributeData.speed
                : 0f;
        }
    }

    public int Attack
    {
        get
        {
            return Data != null && Data.attributeData != null
                ? Data.attributeData.attack
                : 0;
        }
    }

    public int Defense
    {
        get
        {
            return Data != null && Data.attributeData != null
                ? Data.attributeData.defense
                : 0;
        }
    }

    public float CritRate
    {
        get
        {
            return Data != null && Data.attributeData != null
                ? Data.attributeData.critRate
                : 0f;
        }
    }

    public float CritDamage
    {
        get
        {
            return Data != null && Data.attributeData != null
                ? Data.attributeData.critDamage
                : 1f;
        }
    }

    public float HitRate
    {
        get
        {
            return Data != null && Data.attributeData != null
                ? Data.attributeData.hitRate
                : 1f;
        }
    }

    public float DodgeRate
    {
        get
        {
            return Data != null && Data.attributeData != null
                ? Data.attributeData.dodgeRate
                : 0f;
        }
    }

    public void Init(PlayerData data, string runtimeId)
    {
        Data = data;
        RuntimeId = runtimeId;
    }

    // 最小快照能力：与现有保存链对齐（不改链路，只提供便捷入口）
    public void CaptureRuntimeSnapshot()
    {
        if (Data == null) return;
        if (Data.runtimeData == null) Data.runtimeData = new PlayerRuntimeData();
        var t = transform;
        Vector3 pos = t.position;
        Vector3 rot = t.eulerAngles;
        Data.runtimeData.hasValidPosition = true;
        Data.runtimeData.posX = pos.x;
        Data.runtimeData.posY = pos.y;
        Data.runtimeData.posZ = pos.z;
        Data.runtimeData.rotY = rot.y;
        Data.runtimeData.Scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
    }

    public void ApplyRuntimeSnapshot()
    {
        if (Data == null || Data.runtimeData == null || !Data.runtimeData.hasValidPosition) return;
        transform.position = new Vector3(Data.runtimeData.posX, Data.runtimeData.posY, Data.runtimeData.posZ);
        transform.rotation = Quaternion.Euler(0f, Data.runtimeData.rotY, 0f);
    }

    public void ReceiveDamage(int damage)
    {
        if (Data == null) return;
        if (Data.runtimeData == null) Data.runtimeData = new PlayerRuntimeData();
        if (IsDead) return;
        Data.runtimeData.currentHp = Mathf.Max(0, Data.runtimeData.currentHp - damage);
        if (Data.runtimeData.currentHp <= 0)
        {
            Data.runtimeData.isDead = true;
            Debug.Log("[PlayerEntity] Dead");
        }
    }
}
