using UnityEngine;
using UnityEngine.UI;

public class BeginPanel : BasePanel
{
    public override UILayer Layer => UILayer.Normal;

    [Header("Buttons")]
    public Button btnStart;
    public Button btnContinue;
    public Button btnSetting;
    public Button btnAbout;
    public Button btnExit;

    /// <summary>
    /// 每次回到开始界面时重置按钮状态
    /// </summary>
    public void ResetPanelState()
    {
        if (btnStart != null)
            btnStart.interactable = true;

        if (btnContinue != null)
            btnContinue.interactable = true;

        if (btnSetting != null)
            btnSetting.interactable = true;

        if (btnAbout != null)
            btnAbout.interactable = true;

        if (btnExit != null)
            btnExit.interactable = true;

        Debug.Log("[BeginPanel] Reset");
    }

    /// <summary>
    /// 根据是否有任意存档，刷新 Continue 按钮
    /// </summary>
    private void RefreshContinueButtonState()
    {
        if (btnContinue != null)
        {
            btnContinue.interactable = DataManager.Instance.HasAnyPlayerSave(100);
        }
    }

    /// <summary>
    /// 如果开始界面依赖相机初始状态，这里做恢复
    /// </summary>
    private void ResetCameraIfNeeded()
    {
        CameraEvent cameraEvent = Camera.main != null ? Camera.main.GetComponent<CameraEvent>() : null;
        if (cameraEvent != null)
        {
            cameraEvent.ResetCamera();
        }
    }

    protected override void OnCreate()
    {
        if (btnStart != null) btnStart.onClick.AddListener(OnClickStartGame);
        if (btnContinue != null) btnContinue.onClick.AddListener(OnClickContinueGame);
        if (btnSetting != null) btnSetting.onClick.AddListener(OnClickSetting);
        if (btnAbout != null) btnAbout.onClick.AddListener(OnClickAbout);
        if (btnExit != null) btnExit.onClick.AddListener(OnClickExitGame);
    }

    protected override void OnShow()
    {
        base.OnShow();

        ResetPanelState();
        RefreshContinueButtonState();
        ResetCameraIfNeeded();
    }

    protected override void OnDestroyPanel()
    {
        if (btnStart != null) btnStart.onClick.RemoveListener(OnClickStartGame);
        if (btnContinue != null) btnContinue.onClick.RemoveListener(OnClickContinueGame);
        if (btnSetting != null) btnSetting.onClick.RemoveListener(OnClickSetting);
        if (btnAbout != null) btnAbout.onClick.RemoveListener(OnClickAbout);
        if (btnExit != null) btnExit.onClick.RemoveListener(OnClickExitGame);

        base.OnDestroyPanel();
    }

    private void OnClickStartGame()
    {
        Debug.Log("点击了 开始游戏");

        if (btnStart != null)
            btnStart.interactable = false;

        // 先隐藏开始面板，只保留摄像头画面
        UIManager.Instance.HidePanel("BeginPanel", useFade: false);

        CameraEvent cameraEvent = Camera.main != null ? Camera.main.GetComponent<CameraEvent>() : null;

        if (cameraEvent != null)
        {
            cameraEvent.TurnAround(() =>
            {
                Debug.Log("摄像头动画播放完毕，进入创建角色界面");
                EventBus.Publish(new OpenMainPageEvent("CreateRolePanel", hideOld: false, useFade: false));
            });
        }
        else
        {
            Debug.LogWarning("[BeginPanel] 未找到 CameraEvent，直接打开 CreateRolePanel");
            EventBus.Publish(new OpenMainPageEvent("CreateRolePanel", hideOld: false, useFade: false));
        }
    }

    private void OnClickContinueGame()
    {
        // 多存档版本：这里只负责打开 ContinuePanel
        if (!DataManager.Instance.HasAnyPlayerSave(100))
        {
            MessageTipPanel panel = UIManager.Instance.ShowPanel<MessageTipPanel>();
            if (panel != null)
            {
                panel.SetMessage("当前没有游戏存档");
            }

            RefreshContinueButtonState();
            return;
        }

        EventBus.Publish(new OpenPanelEvent("ContinuePanel"));
    }

    private void OnClickSetting()
    {
        Debug.Log("点击了 设置");
        EventBus.Publish(new OpenPanelEvent("SettingPanel"));
    }

    private void OnClickAbout()
    {
        Debug.Log("点击了 关于");
        EventBus.Publish(new OpenPanelEvent("AboutPanel"));
    }

    private void OnClickExitGame()
    {
        UIManager.Instance.ShowConfirm(
        "是否退出游戏？",
        () =>
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        },
        null
    );
    }
}
