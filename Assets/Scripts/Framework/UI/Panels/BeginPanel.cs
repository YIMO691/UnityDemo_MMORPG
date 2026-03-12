using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class BeginPanel : BasePanel
{
    public override UILayer Layer => UILayer.Normal;

    [Header("Buttons")]
    public Button btnStart;
    public Button btnContinue;
    public Button btnSetting;
    public Button btnAbout;
    public Button btnExit;

    [Header("Scene Name")]
    [SerializeField] private string gameSceneName = "SampleScene";

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

    protected override void OnCreate()
    {
        btnStart.onClick.AddListener(OnClickStartGame);
        btnContinue.onClick.AddListener(OnClickContinueGame);
        btnSetting.onClick.AddListener(OnClickSetting);
        btnAbout.onClick.AddListener(OnClickAbout);
        btnExit.onClick.AddListener(OnClickExitGame);
    }

    protected override void OnShow()
    {
       ResetPanelState();
        // 例如后续可以在这里刷新 Continue 按钮状态
        // btnContinue.interactable = SaveSystem.HasSaveData();
    }



    protected override void OnDestroyPanel()
    {
        btnStart.onClick.RemoveListener(OnClickStartGame);
        btnContinue.onClick.RemoveListener(OnClickContinueGame);
        btnSetting.onClick.RemoveListener(OnClickSetting);
        btnAbout.onClick.RemoveListener(OnClickAbout);
        btnExit.onClick.RemoveListener(OnClickExitGame);
    }

    private void OnClickStartGame()
    {
        Debug.Log("点击了 开始游戏");

        btnStart.interactable = false;

        // 先隐藏开始面板，只保留摄像头画面
        UIManager.Instance.HidePanel<BeginPanel>(useFade: false);

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
        Debug.Log("点击了 继续游戏");
        // 后续接存档系统
    }

    private void OnClickSetting()
    {
        Debug.Log("点击了 设置");
        EventBus.Publish(new OpenPanelEvent("SettingPanel"));
    }

    private void OnClickAbout()
    {
        Debug.Log("点击了 关于");
        // 后续这里打开 AboutPanel
    }

    private void OnClickExitGame()
    {
        Debug.Log("点击了 退出游戏");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
