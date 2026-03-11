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

    /// <summary>
    /// 只执行一次：注册按钮事件
    /// </summary>
    protected override void OnCreate()
    {
        btnStart.onClick.AddListener(OnClickStartGame);
        btnContinue.onClick.AddListener(OnClickContinueGame);
        btnSetting.onClick.AddListener(OnClickSetting);
        btnAbout.onClick.AddListener(OnClickAbout);
        btnExit.onClick.AddListener(OnClickExitGame);
    }

    /// <summary>
    /// 每次显示时调用
    /// 目前主菜单没有复杂刷新需求，可以先空着
    /// </summary>
    protected override void OnShow()
    {
        // 例如后续可以在这里刷新 Continue 按钮状态
        // btnContinue.interactable = SaveSystem.HasSaveData();
    }

    /// <summary>
    /// 销毁前移除事件
    /// </summary>
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

        Camera.main.GetComponent<CameraEvent>().TurnAround(() =>
        {
            Debug.Log("进入创建角色界面");
        });

        UIManager.Instance.HidePanel<BeginPanel>(false);

        // SceneManager.LoadScene(gameSceneName);
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
