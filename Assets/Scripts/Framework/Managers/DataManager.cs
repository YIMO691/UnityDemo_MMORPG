using System.Collections.Generic;
using UnityEngine;

// Description: 数据管理器，负责管理游戏中的数据，如设置数据等
public class DataManager
{
    // 单例模式，确保全局只有一个数据管理器实例
    private static DataManager instance = new DataManager();
    public static DataManager Instance => instance;

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

    public bool IsInited => isInited;

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

    #region 玩家数据接口（槽位与文件管理，业务数据移至 GamePlayerDataService）

    public void Clear()
    {
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
    /// 获取下一个可用槽位ID（前移策略）
    /// 不在中间空槽插入新存档，始终在当前最大槽位之后新增
    /// </summary>
    public int GetNextAvailableSlotId(int searchMax = 999)
    {
        return GetMaxUsedSlotId(searchMax) + 1;
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

        if (currentSlotId == slotId)
        {
            currentSlotId = -1;
        }

        Debug.Log("[DataManager] 删除存档槽位：" + slotId);
    }

    #endregion

}
