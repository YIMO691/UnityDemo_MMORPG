using UnityEngine;

public class GameManager : MonoBehaviour
{
    private static GameManager instance;
    public static GameManager Instance => instance;

    private bool initialized;
    private bool pendingEnterBegin;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }

    public void Initialize()
    {
        if (initialized) return;
        Debug.Log("[GameManager] Initialize start");
        InitCoreManagers();
        EnterBeginFlow();
        initialized = true;
        Debug.Log("[GameManager] Initialize success");
    }

    private void InitCoreManagers()
    {
        DataManager.Instance.Init();
        Debug.Log("[GameManager] Init DataManager success");

        UIManager.Instance.Init();
        Debug.Log("[GameManager] Init UIManager success");

        RoleDataManager.Instance.Init();
        Debug.Log("[GameManager] Init RoleDataManager success");

        CreateRoleFlowController.Instance.Init();
        Debug.Log("[GameManager] Init CreateRoleFlowController success");

        RoleUIController.Instance.Init();
        Debug.Log("[GameManager] Init RoleUIController success");
    }

    private void EnterBeginFlow()
    {
        UIManager.Instance.ShowMainPage(UIRouteNames.BeginPanel, hideOld: false, useFade: false);
        Debug.Log("[GameManager] EnterBeginFlow success");
    }

    public void ReturnToBeginFlow()
    {
        MiniMapService.Instance.Clear();
        pendingEnterBegin = true;
        SceneNavigator.EnterBeginScene();
    }

    public void Shutdown()
    {
        // 预留：按需清理全局服务或运行态（当前不做具体操作）
    }

    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        if (scene.name == SceneNames.BeginScene)
        {
            if (UIManager.Instance.IsInited)
            {
                EnterBeginFlow();
            }
            else
            {
                UIManager.Instance.Init();
                EnterBeginFlow();
            }
            pendingEnterBegin = false;
        }
        else if (pendingEnterBegin)
        {
            // 若场景名不匹配但仍标记为 pending，尝试进入 Begin 流（防御性处理）
            if (UIManager.Instance.IsInited)
                EnterBeginFlow();
            pendingEnterBegin = false;
        }
    }
}
