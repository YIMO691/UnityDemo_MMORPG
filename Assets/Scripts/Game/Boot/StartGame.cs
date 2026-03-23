using UnityEngine;

// 游戏启动入口：初始化 UIManager、创角流程控制器，并显示主菜单界面
public class StartGame : MonoBehaviour
{
    private void Start()
    {
        if (GameManager.Instance == null)
        {
            var go = new GameObject("[GameManager]");
            go.AddComponent<GameManager>();
        }
        GameManager.Instance.Initialize();
    }
}
