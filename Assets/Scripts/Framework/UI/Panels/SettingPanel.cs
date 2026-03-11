using UnityEngine;
using UnityEngine.UI;

public class SettingPanel : BasePanel
{
    public override UILayer Layer => UILayer.Popup;

    [Header("音乐和音效设置")]
    public Toggle togMusic;
    public Toggle togSound;

    public Slider sliderMusic;
    public Slider sliderSound;

    public Button btnClose;

    private bool isInitializing = false;

    // 重写属性，设置使用遮罩，点击遮罩关闭面板
    public override bool UseMask => true;
    public override bool CloseByMask => true;

    /// <summary>
    /// 只执行一次：注册事件
    /// </summary>
    protected override void OnCreate()
    {
        togMusic.onValueChanged.AddListener(OnMusicToggleChanged);
        togSound.onValueChanged.AddListener(OnSoundToggleChanged);

        sliderMusic.onValueChanged.AddListener(OnMusicSliderChanged);
        sliderSound.onValueChanged.AddListener(OnSoundSliderChanged);

        btnClose.onClick.AddListener(OnClickClose);
    }

    /// <summary>
    /// 每次打开面板时执行
    /// </summary>
    protected override void OnShow()
    {
        isInitializing = true;

        // 读取数据
        togMusic.isOn = DataManager.Instance.GetMusicOn();
        togSound.isOn = DataManager.Instance.GetSoundOn();

        sliderMusic.value = DataManager.Instance.GetMusicVolume();
        sliderSound.value = DataManager.Instance.GetSoundVolume();

        RefreshMusicState();
        RefreshSoundState();

        isInitializing = false;
    }

    /// <summary>
    /// 销毁面板时取消监听
    /// </summary>
    protected override void OnDestroyPanel()
    {
        togMusic.onValueChanged.RemoveListener(OnMusicToggleChanged);
        togSound.onValueChanged.RemoveListener(OnSoundToggleChanged);

        sliderMusic.onValueChanged.RemoveListener(OnMusicSliderChanged);
        sliderSound.onValueChanged.RemoveListener(OnSoundSliderChanged);

        btnClose.onClick.RemoveListener(OnClickClose);
    }

    // =============================
    // 事件逻辑
    // =============================

    private void OnMusicToggleChanged(bool isOn)
    {
        if (isInitializing) return;

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

    private void OnSoundToggleChanged(bool isOn)
    {
        if (isInitializing) return;

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

    private void OnMusicSliderChanged(float value)
    {
        if (isInitializing) return;

        DataManager.Instance.SetMusicVolume(value, false);
        AudioManager.Instance.SetMusicVolume(value);

        RefreshMusicState();
        DataManager.Instance.SaveSettingData();
    }

    private void OnSoundSliderChanged(float value)
    {
        if (isInitializing) return;

        DataManager.Instance.SetSoundVolume(value, false);
        AudioManager.Instance.SetSoundVolume(value);

        RefreshSoundState();
        DataManager.Instance.SaveSettingData();
    }

    private void RefreshMusicState()
    {
        sliderMusic.interactable = togMusic.isOn;
    }

    private void RefreshSoundState()
    {
        sliderSound.interactable = togSound.isOn;
    }

    private void OnClickClose()
    {
        DataManager.Instance.SaveSettingData();
        EventBus.Publish(new ClosePanelEvent("SettingPanel"));
    }
}
