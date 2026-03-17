using System.Collections.Generic;
using UnityEngine;

// Description: 数据管理器，负责管理游戏中的数据，如设置数据等
public class DataManager
{
    // 单例模式，确保全局只有一个数据管理器实例
    private static DataManager instance = new DataManager();
    public static DataManager Instance => instance;

    private PlayerData currentPlayerData;
    private int currentSlotId = -1;
    private bool isInited = false;

    // 设置数据文件名
    private const string SETTING_FILE_NAME = "setting";

    // 玩家存档文件名前缀
    private const string PLAYER_FILE_PREFIX = "player_";

    // 默认存档槽位数量
    private const int DEFAULT_MAX_SLOT_COUNT = 3;

    public SettingData SettingData { get; private set; }

    // 私有构造函数，防止外部实例化
    private DataManager() { }

    public void Init()
    {
        if (isInited) return;
        LoadSettingData();
        isInited = true;
    }

    /// <summary>
    /// 加载设置数据
    /// </summary>
    public void LoadSettingData()
    {
        SettingData = JsonMgr.Instance.LoadData<SettingData>(SETTING_FILE_NAME);

        if (SettingData == null)
        {
            SettingData = new SettingData();
            SaveSettingData();
        }
    }

    /// <summary>
    /// 保存设置数据
    /// </summary>
    public void SaveSettingData()
    {
        JsonMgr.Instance.SaveData(SettingData, SETTING_FILE_NAME);
    }

    #region 设置音乐和音效的接口，方便外部调用
    public bool GetMusicOn()
    {
        return SettingData.musicOn;
    }

    public bool GetSoundOn()
    {
        return SettingData.soundOn;
    }

    public float GetMusicVolume()
    {
        return SettingData.musicVolume;
    }

    public float GetSoundVolume()
    {
        return SettingData.soundVolume;
    }

    public float GetLastMusicVolume()
    {
        return SettingData.lastMusicVolume;
    }

    public float GetLastSoundVolume()
    {
        return SettingData.lastSoundVolume;
    }

    public void SetMusicOn(bool value, bool autoSave = true)
    {
        SettingData.musicOn = value;
        if (autoSave)
            SaveSettingData();
    }

    public void SetSoundOn(bool value, bool autoSave = true)
    {
        SettingData.soundOn = value;
        if (autoSave)
            SaveSettingData();
    }

    public void SetMusicVolume(float value, bool autoSave = true)
    {
        SettingData.musicVolume = value;

        if (value > 0.0001f)
            SettingData.lastMusicVolume = value;

        if (autoSave)
            SaveSettingData();
    }

    public void SetSoundVolume(float value, bool autoSave = true)
    {
        SettingData.soundVolume = value;

        if (value > 0.0001f)
            SettingData.lastSoundVolume = value;

        if (autoSave)
            SaveSettingData();
    }
    #endregion

    #region 玩家数据接口
    public void SetCurrentPlayerData(PlayerData playerData)
    {
        currentPlayerData = playerData;
    }

    public PlayerData GetCurrentPlayerData()
    {
        return currentPlayerData;
    }

    public bool HasCurrentPlayerData()
    {
        return currentPlayerData != null;
    }

    /// <summary>
    /// 清除当前内存中的玩家数据
    /// </summary>
    public void ClearCurrentPlayerData()
    {
        currentPlayerData = null;
        currentSlotId = -1;
    }

    public void Clear()
    {
        currentPlayerData = null;
        currentSlotId = -1;
        isInited = false;
    }

    public void SetCurrentSlotId(int slotId)
    {
        currentSlotId = slotId;
    }

    public int GetCurrentSlotId()
    {
        return currentSlotId;
    }

    public void ClearCurrentSlotId()
    {
        currentSlotId = -1;
    }

    #endregion

    #region 多存档槽位接口

    /// <summary>
    /// 获取存档文件名
    /// 例如：player_1、player_2、player_3
    /// </summary>
    public string GetPlayerSlotFileName(int slotId)
    {
        return PLAYER_FILE_PREFIX + slotId;
    }

    /// <summary>
    /// 将指定玩家数据保存到指定槽位
    /// </summary>
    public void SavePlayerDataToSlot(int slotId, PlayerData playerData)
    {
        if (slotId < 1)
        {
            Debug.LogError($"[DataManager] 非法槽位ID: {slotId}");
            return;
        }
        if (playerData == null)
        {
            Debug.LogWarning("[DataManager] 玩家数据为空，无法保存。");
            return;
        }

        playerData.saveTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        string fileName = GetPlayerSlotFileName(slotId);
        JsonMgr.Instance.SaveData(playerData, fileName);

        DataManager.Instance.SetCurrentSlotId(slotId);

        Debug.Log("[DataManager] 玩家存档保存成功，槽位：" + slotId);
    }


    /// <summary>
    /// 将当前玩家数据保存到指定槽位
    /// </summary>
    public void SaveCurrentPlayerDataToSlot(int slotId)
    {
        if (currentPlayerData == null)
        {
            Debug.LogWarning("[DataManager] 当前没有玩家数据，无法保存。");
            return;
        }

        SavePlayerDataToSlot(slotId, currentPlayerData);
    }

    /// <summary>
    /// 从指定槽位读取玩家数据，并赋值给 currentPlayerData
    /// </summary>
    public bool LoadPlayerDataFromSlot(int slotId)
    {
        if (slotId < 1)
        {
            Debug.LogError($"[DataManager] 非法槽位ID: {slotId}");
            return false;
        }
        if (!HasPlayerSaveInSlot(slotId))
        {
            Debug.LogWarning("[DataManager] 槽位 " + slotId + " 存档不存在。");
            return false;
        }

        string fileName = GetPlayerSlotFileName(slotId);
        PlayerData playerData = JsonMgr.Instance.LoadData<PlayerData>(fileName);

        if (playerData == null || playerData.baseData == null)
        {
            Debug.LogWarning("[DataManager] 槽位 " + slotId + " 存档读取失败。");
            return false;
        }

        currentPlayerData = playerData;
        currentSlotId = slotId;

        Debug.Log("[DataManager] 槽位 " + slotId + " 读档成功。");
        return true;
    }


    /// <summary>
    /// 获取指定槽位的玩家数据（不影响 currentPlayerData）
    /// </summary>
    public PlayerData GetPlayerDataFromSlot(int slotId)
    {
        if (slotId < 1)
        {
            return null;
        }
        if (!HasPlayerSaveInSlot(slotId))
        {
            return null;
        }

        string fileName = GetPlayerSlotFileName(slotId);
        return JsonMgr.Instance.LoadData<PlayerData>(fileName);
    }

    /// <summary>
    /// 指定槽位是否存在存档
    /// </summary>
    public bool HasPlayerSaveInSlot(int slotId)
    {
        string fileName = GetPlayerSlotFileName(slotId);
        return JsonMgr.Instance.HasData(fileName);
    }

    /// <summary>
    /// 是否存在任意玩家存档
    /// </summary>
    public bool HasAnyPlayerSave(int maxSlotCount = DEFAULT_MAX_SLOT_COUNT)
    {
        for (int i = 1; i <= maxSlotCount; i++)
        {
            if (HasPlayerSaveInSlot(i))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 获取当前已存在的最大槽位ID
    /// 如果没有存档，返回 0
    /// </summary>
    public int GetMaxUsedSlotId(int searchMax = 999)
    {
        int maxSlotId = 0;

        for (int i = 1; i <= searchMax; i++)
        {
            if (HasPlayerSaveInSlot(i))
            {
                maxSlotId = i;
            }
        }

        return maxSlotId;
    }

    /// <summary>
    /// 获取下一个可用槽位ID
    /// 优先复用空槽位；如果没有空槽位，则追加到末尾
    /// </summary>
    public int GetNextAvailableSlotId(int searchMax = 999)
    {
        // 先找空槽位
        for (int i = 1; i <= searchMax; i++)
        {
            if (!HasPlayerSaveInSlot(i))
            {
                return i;
            }
        }

        // 如果 searchMax 范围内都满了，就继续追加
        return GetMaxUsedSlotId(searchMax) + 1;
    }

    /// <summary>
    /// 获取指定槽位的存档摘要信息，用于 ContinuePanel 显示
    /// </summary>
    public PlayerSaveMetaData GetPlayerSaveMetaData(int slotId)
    {
        PlayerData playerData = GetPlayerDataFromSlot(slotId);
        if (playerData == null)
        {
            return null;
        }

        if (playerData.baseData == null || playerData.progressData == null)
        {
            return null;
        }

        PlayerSaveMetaData metaData = new PlayerSaveMetaData();
        metaData.slotId = slotId;
        metaData.roleName = string.IsNullOrEmpty(playerData.baseData.roleName) ? "未命名角色" : playerData.baseData.roleName;
        metaData.classId = playerData.baseData.classId;
        metaData.level = playerData.progressData.level;
        metaData.saveTime = string.IsNullOrEmpty(playerData.saveTime) ? "--" : playerData.saveTime;

        return metaData;
    }

    /// <summary>
    /// 获取所有有存档的槽位摘要
    /// </summary>
    public List<PlayerSaveMetaData> GetAllPlayerSaveMetaData(int maxSlotCount = 20)
    {
        List<PlayerSaveMetaData> list = new List<PlayerSaveMetaData>();

        for (int i = 1; i <= maxSlotCount; i++)
        {
            PlayerSaveMetaData metaData = GetPlayerSaveMetaData(i);
            if (metaData != null)
            {
                list.Add(metaData);
            }
        }

        return list;
    }

    /// <summary>
    /// 删除指定槽位存档
    /// </summary>
    public void DeletePlayerDataInSlot(int slotId)
    {
        if (slotId < 1)
        {
            Debug.LogError($"[DataManager] 非法槽位ID: {slotId}");
            return;
        }
        string fileName = GetPlayerSlotFileName(slotId);

        JsonMgr.Instance.DeleteData(fileName);

        Debug.Log("[DataManager] 删除存档槽位：" + slotId);
    }

    #endregion

    #region 兼容旧单存档接口（可逐步废弃）

    /// <summary>
    /// 兼容旧逻辑：默认保存到槽位1
    /// </summary>
    public void SaveCurrentPlayerData()
    {
        SaveCurrentPlayerDataToSlot(1);
    }

    /// <summary>
    /// 兼容旧逻辑：默认从槽位1读取
    /// </summary>
    public bool LoadCurrentPlayerData()
    {
        return LoadPlayerDataFromSlot(1);
    }

    /// <summary>
    /// 兼容旧逻辑：默认检查槽位1
    /// </summary>
    public bool HasLocalPlayerSave()
    {
        return HasPlayerSaveInSlot(1);
    }

    #endregion
}
