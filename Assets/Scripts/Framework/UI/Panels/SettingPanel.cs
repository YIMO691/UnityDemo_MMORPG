using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Game.Runtime;

public class SettingPanel : BasePanel
{
    public override UILayer Layer => UILayer.Popup;
    public override bool UseMask => true;
    public override bool CloseByMask => true;

    [Header("音乐和音效设置")]
    public Toggle togMusic;
    public Toggle togSound;

    public Slider sliderMusic;
    public Slider sliderSound;

    [Header("系统操作")]
    public Button btnBackToTitle;
    public Button btnQuitGame;

    [Header("通用")]
    public Button btnClose;

    private bool isInitializing = false;

    protected override void OnCreate()
    {
        if (togMusic != null) togMusic.onValueChanged.AddListener(OnMusicToggleChanged);
        if (togSound != null) togSound.onValueChanged.AddListener(OnSoundToggleChanged);

        if (sliderMusic != null) sliderMusic.onValueChanged.AddListener(OnMusicSliderChanged);
        if (sliderSound != null) sliderSound.onValueChanged.AddListener(OnSoundSliderChanged);

        if (btnBackToTitle != null) btnBackToTitle.onClick.AddListener(OnClickBackToTitle);
        if (btnQuitGame != null) btnQuitGame.onClick.AddListener(OnClickQuitGame);
        if (btnClose != null) btnClose.onClick.AddListener(OnClickClose);
    }

    protected override void OnShow()
    {
        isInitializing = true;

        if (togMusic != null)
            togMusic.isOn = DataManager.Instance.GetMusicOn();

        if (togSound != null)
            togSound.isOn = DataManager.Instance.GetSoundOn();

        if (sliderMusic != null)
            sliderMusic.value = DataManager.Instance.GetMusicVolume();

        if (sliderSound != null)
            sliderSound.value = DataManager.Instance.GetSoundVolume();

        RefreshMusicState();
        RefreshSoundState();

        isInitializing = false;
    }

    protected override void OnDestroyPanel()
    {
        if (togMusic != null) togMusic.onValueChanged.RemoveListener(OnMusicToggleChanged);
        if (togSound != null) togSound.onValueChanged.RemoveListener(OnSoundToggleChanged);

        if (sliderMusic != null) sliderMusic.onValueChanged.RemoveListener(OnMusicSliderChanged);
        if (sliderSound != null) sliderSound.onValueChanged.RemoveListener(OnSoundSliderChanged);

        if (btnBackToTitle != null) btnBackToTitle.onClick.RemoveListener(OnClickBackToTitle);
        if (btnQuitGame != null) btnQuitGame.onClick.RemoveListener(OnClickQuitGame);
        if (btnClose != null) btnClose.onClick.RemoveListener(OnClickClose);
    }

    private void OnMusicToggleChanged(bool isOn)
    {
        if (isInitializing) return;

        DataManager.Instance.SetMusicOn(isOn, false);

        if (isOn)
        {
            float value = DataManager.Instance.GetLastMusicVolume();
            if (value <= 0.0001f)
                value = 1f;

            if (sliderMusic != null)
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

            if (sliderSound != null)
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
        if (sliderMusic != null && togMusic != null)
            sliderMusic.interactable = togMusic.isOn;
    }

    private void RefreshSoundState()
    {
        if (sliderSound != null && togSound != null)
            sliderSound.interactable = togSound.isOn;
    }

    private void OnClickBackToTitle()
    {
        UIManager.Instance.ShowConfirm(
            "是否返回开始界面？",
            () =>
            {
                DataManager.Instance.SaveSettingData();

                GameSceneEntry entry = Object.FindObjectOfType<GameSceneEntry>();
                if (entry != null)
                {
                    entry.SaveCurrentPlayerTransform();
                }

                EventBus.Publish(new ClosePanelEvent(UIRouteNames.SettingPanel));

                MainPanel mainPanel = UIManager.Instance.GetPanel<MainPanel>();
                if (mainPanel != null)
                {
                    UIManager.Instance.HidePanel<MainPanel>(useFade: false);
                }

                DataManager.Instance.ClearCurrentPlayerData();
                Game.Runtime.GameRuntime.CurrentPlayerData = null;

                SceneNavigator.EnterBeginScene();
            },
            null
        );
    }

    private void OnClickQuitGame()
    {
        UIManager.Instance.ShowConfirm(
            "是否退出游戏？",
            () =>
            {
                DataManager.Instance.SaveSettingData();

                GameSceneEntry entry = Object.FindObjectOfType<GameSceneEntry>();
                if (entry != null)
                {
                    entry.SaveCurrentPlayerTransform();
                }

#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
            },
            null
        );
    }



    private void OnClickClose()
    {
        DataManager.Instance.SaveSettingData();
        EventBus.Publish(new ClosePanelEvent(UIRouteNames.SettingPanel));
    }
}
