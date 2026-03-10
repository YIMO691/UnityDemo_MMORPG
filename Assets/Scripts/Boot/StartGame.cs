using UnityEngine;

// 这个脚本的作用是游戏启动时初始化 UI 管理器，并显示主菜单界面（BeginPanel）。它应该挂载在一个场景中的 GameObject 上，通常是一个空的 GameObject，作为游戏的入口点。
public class StartGame : MonoBehaviour
{
    private void Start()
    {
        UIManager.Instance.Init();
        UIManager.Instance.ShowPanel<BeginPanel>();
    }
}
