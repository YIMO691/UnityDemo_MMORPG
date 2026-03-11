using UnityEngine;
using UnityEngine.UI;

public class SettingPanel : BasePanel
{
    // 重写 Layer 属性，指定该面板属于 Popup 层
    public override UILayer Layer => UILayer.Popup;

    [Header("音乐和音效设置")]
    public Toggle togMusic;
    public Toggle togSound;

    public Slider sliderMusic;
    public Slider sliderSound;

    public Button btnClose;

    private bool isInitializing = false;

    public override void Init()
    {
        isInitializing = true;

        // 读取数据
        togMusic.isOn = DataManager.Instance.GetMusicOn();
        togSound.isOn = DataManager.Instance.GetSoundOn();

        sliderMusic.value = DataManager.Instance.GetMusicVolume();
        sliderSound.value = DataManager.Instance.GetSoundVolume();

        // 根据 Toggle 状态更新 Slider 是否可用
        RefreshMusicState();
        RefreshSoundState();

        // 注册事件
        togMusic.onValueChanged.AddListener(OnMusicToggleChanged);
        togSound.onValueChanged.AddListener(OnSoundToggleChanged);
        sliderMusic.onValueChanged.AddListener(OnMusicSliderChanged);
        sliderSound.onValueChanged.AddListener(OnSoundSliderChanged);
        btnClose.onClick.AddListener(OnClickClose);

        isInitializing = false;
    }

    // 控制音乐开关的事件处理
    private void OnMusicToggleChanged(bool isOn)
    {
        if (isInitializing)
            return;

        DataManager.Instance.SetMusicOn(isOn, false);

        if (isOn)
        {
            float value = DataManager.Instance.GetLastMusicVolume();
            if (value <= 0.0001f)
                value = 1f;

            sliderMusic.value = value;
            DataManager.Instance.SetMusicVolume(value, false);
            AudioManager.Instance.SetMusicVolume(value);
        }
        else
        {
            DataManager.Instance.SetMusicVolume(0f, false);
            AudioManager.Instance.SetMusicVolume(0f);
        }

        RefreshMusicState();
        DataManager.Instance.SaveSettingData();
    }

    // 控制音效开关的事件处理
    private void OnSoundToggleChanged(bool isOn)
    {
        if (isInitializing)
            return;

        DataManager.Instance.SetSoundOn(isOn, false);

        if (isOn)
        {
            float value = DataManager.Instance.GetLastSoundVolume();
            if (value <= 0.0001f)
                value = 1f;

            sliderSound.value = value;
            DataManager.Instance.SetSoundVolume(value, false);
            AudioManager.Instance.SetSoundVolume(value);
        }
        else
        {
            DataManager.Instance.SetSoundVolume(0f, false);
            AudioManager.Instance.SetSoundVolume(0f);
        }

        RefreshSoundState();
        DataManager.Instance.SaveSettingData();
    }

    // 控制音乐音量的事件处理
    private void OnMusicSliderChanged(float value)
    {
        if (isInitializing)
            return;

        // 当滑条值非常小（接近0）时，认为是关闭状态
        bool isOn = value > 0.0001f;

        DataManager.Instance.SetMusicVolume(value, false);

        AudioManager.Instance.SetMusicVolume(value);
        RefreshMusicState();
        DataManager.Instance.SaveSettingData();
    }

    // 控制音效音量的事件处理
    private void OnSoundSliderChanged(float value)
    {
        if (isInitializing)
            return;

        bool isOn = value > 0.0001f;

        DataManager.Instance.SetSoundVolume(value, false);

        AudioManager.Instance.SetSoundVolume(value);
        RefreshSoundState();
        DataManager.Instance.SaveSettingData();
    }

    // 刷新音乐开关状态，控制音乐音量滑条是否可用
    private void RefreshMusicState()
    {
        sliderMusic.interactable = togMusic.isOn;
    }

    // 刷新音效开关状态，控制音效音量滑条是否可用
    private void RefreshSoundState()
    {
        sliderSound.interactable = togSound.isOn;
    }

    // 关闭设置界面，保存设置数据
    private void OnClickClose()
    {
        DataManager.Instance.SaveSettingData();
        EventBus.Publish(new ClosePanelEvent("SettingPanel"));
    }
}
