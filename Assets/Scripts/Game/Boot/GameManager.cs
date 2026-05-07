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
        RuntimeLifecycleBootstrap.RegisterDefaults();
        RuntimeLifecycleRegistry.Instance.InitAll();
        InitGameOnlyManagers();
        MonsterConfigManager.Instance.Init();
        EnterBeginFlow();
        initialized = true;
        Debug.Log("[GameManager] Initialize success");
    }

    private void InitGameOnlyManagers()
    {
        InventoryUIController.Instance.Init();
        Debug.Log("[GameManager] Init InventoryUIController success");

        ItemDetailUIController.Instance.Init();
        Debug.Log("[GameManager] Init ItemDetailUIController success");
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
        RuntimeLifecycleRegistry.Instance.ShutdownAll();
    }

    private void OnDestroy()
    {
        if (instance != this) return;
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
        instance = null;
    }

    private void OnApplicationQuit()
    {
        Shutdown();
    }

    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        if (scene.name == SceneNames.BeginScene)
        {
            EnterBeginFlow();
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
