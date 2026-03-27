using System.Collections.Generic;
using UnityEngine;

public class GamePlayerDataService
{
    private static readonly GamePlayerDataService instance = new GamePlayerDataService();
    public static GamePlayerDataService Instance => instance;

    private const int DefaultInventorySlotCount = 50;

    private PlayerData currentPlayerData;

    private GamePlayerDataService() { }

    public PlayerData GetCurrentPlayerData()
    {
        return currentPlayerData;
    }

    public void SetCurrentPlayerData(PlayerData playerData)
    {
        EnsurePlayerDataSchema(playerData);
        currentPlayerData = playerData;
    }

    public void ClearCurrent()
    {
        currentPlayerData = null;
        DataManager.Instance.ClearCurrentSlotId();
    }

    public int GetCurrentSlotId()
    {
        return DataManager.Instance.GetCurrentSlotId();
    }

    public bool HasCurrentSession()
    {
        return currentPlayerData != null && GetCurrentSlotId() > 0;
    }

    public void SavePlayerDataToSlot(int slotId, PlayerData playerData)
    {
        if (slotId < 1)
        {
            Debug.LogError($"[GamePlayerDataService] 非法槽位ID: {slotId}");
            return;
        }
        if (playerData == null)
        {
            Debug.LogWarning("[GamePlayerDataService] 玩家数据为空，无法保存。");
            return;
        }

        EnsurePlayerDataSchema(playerData);
        playerData.saveTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        string fileName = DataManager.Instance.GetPlayerSlotFileName(slotId);
        JsonMgr.Instance.SaveData(playerData, fileName);

        DataManager.Instance.SetCurrentSlotId(slotId);

        Debug.Log("[GamePlayerDataService] 玩家存档保存成功，槽位：" + slotId);
    }

    public void SaveCurrentPlayerDataToSlot(int slotId)
    {
        if (currentPlayerData == null)
        {
            Debug.LogWarning("[GamePlayerDataService] 当前没有玩家数据，无法保存。");
            return;
        }

        SavePlayerDataToSlot(slotId, currentPlayerData);
    }

    public bool SaveCurrentToCurrentSlot()
    {
        if (currentPlayerData == null)
        {
            Debug.LogWarning("[GamePlayerDataService] SaveCurrentToCurrentSlot 失败：当前玩家为空。");
            return false;
        }
        int slotId = GetCurrentSlotId();
        if (slotId < 1)
        {
            Debug.LogWarning("[GamePlayerDataService] SaveCurrentToCurrentSlot 失败：当前槽位非法。");
            return false;
        }
        SavePlayerDataToSlot(slotId, currentPlayerData);
        return true;
    }

    public bool LoadPlayerDataFromSlot(int slotId)
    {
        if (slotId < 1)
        {
            Debug.LogError($"[GamePlayerDataService] 非法槽位ID: {slotId}");
            return false;
        }
        if (!DataManager.Instance.HasPlayerSaveInSlot(slotId))
        {
            Debug.LogWarning("[GamePlayerDataService] 槽位 " + slotId + " 存档不存在。");
            return false;
        }

        string fileName = DataManager.Instance.GetPlayerSlotFileName(slotId);
        PlayerData playerData = JsonMgr.Instance.LoadData<PlayerData>(fileName);

        if (playerData == null || playerData.baseData == null)
        {
            Debug.LogWarning("[GamePlayerDataService] 槽位 " + slotId + " 存档读取失败。");
            return false;
        }

        EnsurePlayerDataSchema(playerData);
        currentPlayerData = playerData;
        DataManager.Instance.SetCurrentSlotId(slotId);

        Debug.Log("[GamePlayerDataService] 槽位 " + slotId + " 读档成功。");
        return true;
    }

    public PlayerData GetPlayerDataFromSlot(int slotId)
    {
        if (slotId < 1) return null;
        if (!DataManager.Instance.HasPlayerSaveInSlot(slotId)) return null;

        string fileName = DataManager.Instance.GetPlayerSlotFileName(slotId);
        PlayerData data = JsonMgr.Instance.LoadData<PlayerData>(fileName);
        EnsurePlayerDataSchema(data);
        return data;
    }

    public bool HasAnyPlayerSave(int maxSlotCount = 50)
    {
        for (int i = 1; i <= maxSlotCount; i++)
        {
            if (DataManager.Instance.HasPlayerSaveInSlot(i))
            {
                return true;
            }
        }
        return false;
    }

    public List<PlayerSaveMetaData> GetAllPlayerSaveMetaData(int maxSlotCount = 50)
    {
        List<PlayerSaveMetaData> list = new List<PlayerSaveMetaData>();
        for (int i = 1; i <= maxSlotCount; i++)
        {
            if (DataManager.Instance.HasPlayerSaveInSlot(i))
            {
                var data = GetPlayerDataFromSlot(i);
                var meta = PlayerSaveMetaMapper.Map(data, i);
                if (meta != null) list.Add(meta);
            }
        }
        return list;
    }

    private void EnsurePlayerDataSchema(PlayerData playerData)
    {
        if (playerData == null) return;

        // ===== 背包修复 =====
        if (playerData.inventoryData == null)
        {
            playerData.inventoryData = CreateDefaultInventoryData();
        }

        if (playerData.inventoryData.slots == null)
        {
            playerData.inventoryData.slots = new List<InventorySlotData>();
        }

        if (playerData.inventoryData.slotCount < DefaultInventorySlotCount)
        {
            playerData.inventoryData.slotCount = DefaultInventorySlotCount;
        }

        // ===== runtimeData 修复 =====
        if (playerData.runtimeData == null)
        {
            playerData.runtimeData = new PlayerRuntimeData();
        }

        // ===== attributeData 修复 =====
        if (playerData.baseData != null)
        {
            var classConfig = RoleDataManager.Instance.GetClassConfig(playerData.baseData.classId);

            if (classConfig == null)
            {
                Debug.LogWarning($"[GamePlayerDataService] 职业配置不存在 classId={playerData.baseData.classId}");
                return;
            }

            if (playerData.attributeData == null)
            {
                playerData.attributeData = new PlayerAttributeData();
            }

            // ===== 关键：补 stamina =====
            if (playerData.attributeData.maxStamina <= 0)
            {
                playerData.attributeData.maxStamina = classConfig.maxStamina;
                Debug.Log("[SchemaFix] 修复 maxStamina");
            }

            if (playerData.attributeData.maxHp <= 0)
            {
                playerData.attributeData.maxHp = classConfig.maxHp;
            }

            if (playerData.attributeData.attack <= 0)
            {
                playerData.attributeData.attack = classConfig.attack;
            }

            if (playerData.attributeData.defense <= 0)
            {
                playerData.attributeData.defense = classConfig.defense;
            }

            if (playerData.attributeData.speed <= 0)
            {
                playerData.attributeData.speed = classConfig.speed;
            }

            // ===== runtime 修复 =====
            if (playerData.runtimeData.currentHp <= 0 && !playerData.runtimeData.isDead)
            {
                playerData.runtimeData.currentHp = playerData.attributeData.maxHp;
            }

            if (playerData.runtimeData.currentStamina <= 0)
            {
                playerData.runtimeData.currentStamina = playerData.attributeData.maxStamina;
                Debug.Log("[SchemaFix] 修复 currentStamina");
            }

            // Clamp
            playerData.runtimeData.currentHp = Mathf.Clamp(
                playerData.runtimeData.currentHp,
                0,
                playerData.attributeData.maxHp
            );

            playerData.runtimeData.currentStamina = Mathf.Clamp(
                playerData.runtimeData.currentStamina,
                0,
                playerData.attributeData.maxStamina
            );
        }
    }

    private InventoryData CreateDefaultInventoryData()
    {
        return new InventoryData
        {
            slotCount = DefaultInventorySlotCount,
            slots = new System.Collections.Generic.List<InventorySlotData>()
        };
    }
}
