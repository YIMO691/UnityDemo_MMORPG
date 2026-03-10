using System;

// 设置数据类，用于保存音乐和音效的开关状态以及音量设置
[Serializable]
public class SettingData
{
    public bool musicOn = true;
    public bool soundOn = true;

    public float musicVolume = 1f;
    public float soundVolume = 1f;

    // 用于关闭后再次开启时恢复
    public float lastMusicVolume = 1f;
    public float lastSoundVolume = 1f;
}
