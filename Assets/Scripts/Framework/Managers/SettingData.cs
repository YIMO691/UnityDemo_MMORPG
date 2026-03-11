/// <summary>
/// 设置数据类，用于存储游戏的设置信息
/// </summary>
[System.Serializable]
public class SettingData
{
    // 音乐开关
    public bool musicOn = true;
    // 音效开关
    public bool soundOn = true;
    // 音乐音量 (0-1)
    public float musicVolume = 1.0f;
    // 音效音量 (0-1)
    public float soundVolume = 1.0f;
}
