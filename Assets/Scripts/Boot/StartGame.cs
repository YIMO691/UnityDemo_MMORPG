using UnityEngine;

// 游戏启动入口：初始化 UIManager、创角流程控制器，并显示主菜单界面
public class StartGame : MonoBehaviour
{
    private void Start()
    {
        UIManager.Instance.Init();
        CreateRoleFlowController.Instance.Init();

        UIManager.Instance.ShowMainPage("BeginPanel", hideOld: false, useFade: false);
    }
}
