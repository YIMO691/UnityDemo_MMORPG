using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class BeginPanel : BasePanel
{
    // 重写 Layer 属性，指定该面板属于 Normal 层
    public override UILayer Layer => UILayer.Normal;

    // 使用 Header 属性在 Inspector 中分组显示按钮，方便编辑和维护
    [Header("Buttons")]
    public Button btnStart;
    public Button btnContinue;
    public Button btnSetting;
    public Button btnAbout;
    public Button btnExit;


    // 场景名称，可以在 Inspector 中设置，方便后续修改
    [Header("Scene Name")]

    // 使用 SerializeField 让私有字段在 Inspector 中可见，同时保持封装性，方便后续修改场景名称
    [SerializeField] private string gameSceneName = "SampleScene";

    public override void Init()
    {
        btnStart.onClick.AddListener(OnClickStartGame);
        btnContinue.onClick.AddListener(OnClickContinueGame);
        btnSetting.onClick.AddListener(OnClickSetting);
        btnAbout.onClick.AddListener(OnClickAbout);
        btnExit.onClick.AddListener(OnClickExitGame);
    }

    private void OnClickStartGame()
    {
        Debug.Log("点击了 开始游戏");

        Camera.main.GetComponent<CameraEvent>().TurnAround(() =>
        {
            print("进入创建角色界面");
        });
        // 如果后面想避免主菜单残留，可以先隐藏自己
        UIManager.Instance.HidePanel<BeginPanel>(false);

        //SceneManager.LoadScene(gameSceneName);
    }

    private void OnClickContinueGame()
    {
        Debug.Log("点击了 继续游戏");
        // 后续这里接存档系统
    }

    private void OnClickSetting()
    {
        Debug.Log("点击了 设置");

        EventBus.Publish(new OpenPanelEvent("SettingPanel"));
    }


    private void OnClickAbout()
    {
        Debug.Log("点击了 关于");
        // 后续这里可以打开 AboutPanel
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

    protected virtual void OnDestroy()
    {
        btnStart.onClick.RemoveListener(OnClickStartGame);
        btnContinue.onClick.RemoveListener(OnClickContinueGame);
        btnSetting.onClick.RemoveListener(OnClickSetting);
        btnAbout.onClick.RemoveListener(OnClickAbout);
        btnExit.onClick.RemoveListener(OnClickExitGame);

    }
}
