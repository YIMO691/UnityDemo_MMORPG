// Description: 数据管理器，负责管理游戏中的数据，如设置数据等
public class DataManager
{
    // 单例模式，确保全局只有一个数据管理器实例
    private static DataManager instance = new DataManager();
    public static DataManager Instance => instance;

    private PlayerData currentPlayerData;

    // 设置数据文件名
    private const string SETTING_FILE_NAME = "setting";

    public SettingData SettingData { get; private set; }

    // 私有构造函数，防止外部实例化
    private DataManager()
    {
        LoadSettingData();
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


}
