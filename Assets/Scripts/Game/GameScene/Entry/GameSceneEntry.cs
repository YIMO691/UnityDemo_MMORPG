
using UnityEngine;

public class GameSceneEntry : MonoBehaviour
{
    [SerializeField] private Transform playerSpawnPoint;

    private GameObject playerInstance;
    private GameObject mainCameraInstance;
    private GameObject followCameraInstance;
    private readonly PlayerSaveService playerSaveService = new PlayerSaveService();

    [SerializeField] private Camera miniMapCamera;
    [SerializeField] private MiniMapCameraController miniMapController;

    private void Start()
    {
        Debug.Log("[GameSceneEntry] Start");
        InitScene();
    }

    private void InitScene()
    {
        Debug.Log("[GameSceneEntry] InitScene begin");

        if (GamePlayerDataService.Instance.GetCurrentPlayerData() == null)
        {
            Debug.LogError("[GameSceneEntry] CurrentPlayerData is null.");
            SceneNavigator.EnterBeginScene();
            return;
        }

        ThirdPersonController controller;

        bool okAssemble = TryAssembleScene(out controller);

        if (!okAssemble)
        {
            CleanupOnFail();
            SceneNavigator.EnterBeginScene();
            return;
        }

        CommitScene(controller);

        if (playerInstance != null)
        {
            var entity = playerInstance.GetComponent<PlayerEntity>();
            if (entity != null)
                PlayerLocator.Instance.Register(entity);
            else
                PlayerLocator.Instance.Register(playerInstance.transform);
            var data = GamePlayerDataService.Instance.GetCurrentPlayerData();
            if (data != null && data.runtimeData != null)
            {
                var active = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
                data.runtimeData.Scene = active;
                var p = playerInstance.transform.position;
                data.runtimeData.posX = p.x;
                data.runtimeData.posY = p.y;
                data.runtimeData.posZ = p.z;
                data.runtimeData.hasValidPosition = true;
            }
            var nav = playerInstance.GetComponent<PlayerNavigator>();
            if (nav == null) nav = playerInstance.AddComponent<PlayerNavigator>();
            nav.SetAgentId(NavigationConsts.PlayerAgentId);
        }

        Debug.Log("[GameSceneEntry] InitScene end");
    }

    private bool TryAssembleScene(out ThirdPersonController controller)
    {
        controller = null;
        bool okCreate = CameraRigAssembler.TryCreate(out mainCameraInstance, out followCameraInstance);
        PlayerEntity playerEntity;
        bool okAssemble = PlayerRuntimeService.CreateRuntimePlayer(
            GamePlayerDataService.Instance.GetCurrentPlayerData(),
            GetSpawnPosition(),
            GetSpawnRotation(),
            out playerInstance,
            out controller,
            out playerEntity);
        bool okBind = okAssemble && followCameraInstance != null && CameraRigAssembler.TryBind(controller, followCameraInstance);
        if (!okBind && okCreate && okAssemble)
        {
            CameraRigAssembler.FallbackBind(controller, mainCameraInstance);
            okBind = true;
        }
        if (!(okCreate && okAssemble && okBind))
        {
            return false;
        }
        MiniMapAssembler.TryInitObjects(ref miniMapCamera, ref miniMapController);
        return true;
    }

    private void CommitScene(ThirdPersonController controller)
    {
        OpenMainPanel();
        MiniMapAssembler.BindTarget(miniMapCamera, miniMapController, playerInstance.transform);
        var data = GamePlayerDataService.Instance.GetCurrentPlayerData();
        RuntimeSceneCommitter.WriteSceneContext(data, playerInstance.transform);
        EnsureDebugCanvas();
        MonsterModule.InitForScene();
        EnsureBattleRuntime();
    }

    private void EnsureDebugCanvas()
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        GameObject existing = GameObject.Find(ObjectNames.DebugCanvasRoot);
        if (existing != null) return;

        GameObject prefab = ResourceManager.Instance.Load<GameObject>(AssetPaths.DebugCanvas);
        if (prefab == null)
        {
            Debug.LogWarning("[GameSceneEntry] DebugCanvas prefab not found: UI/Root/DebugCanvas");
            return;
        }
        GameObject go = Instantiate(prefab);
        go.name = ObjectNames.DebugCanvasRoot;
        // 可按需是否跨场景常驻，这里保持场景级

        // 挂载 PoolMonitorPanel 预制（包含已配置的 itemPrefab 与热键逻辑）
        GameObject monitorPrefab = ResourceManager.Instance.Load<GameObject>(AssetPaths.PoolMonitorPanel);
        if (monitorPrefab == null)
        {
            Debug.LogWarning("[GameSceneEntry] PoolMonitorPanel prefab not found: UI/Windows/PoolMonitorPanel");
            return;
        }
        var monitorObj = Instantiate(monitorPrefab, go.transform, false);
        var monitorRect = monitorObj.GetComponent<RectTransform>();
        if (monitorRect != null)
        {
            monitorRect.anchorMin = new Vector2(0f, 0f);
            monitorRect.anchorMax = new Vector2(1f, 1f);
            monitorRect.offsetMin = Vector2.zero;
            monitorRect.offsetMax = Vector2.zero;
        }
        var monitor = monitorObj.GetComponent<PoolMonitorPanel>();
        if (monitor != null)
        {
            monitor.SetVisible(false);
        }
#endif
    }

    // MiniMap 初始化迁移至 MiniMapAssembler

    

   
    private void EnsureBattleRuntime()
    {
        GameObject existing = GameObject.Find(ObjectNames.BattleRuntime);
        if (existing != null) return;
        GameObject go = new GameObject(ObjectNames.BattleRuntime);
        go.AddComponent<DamageFeedbackListener>();
        if (!PoolManager.Instance.Contains(PoolKey.DamageText))
        {
            var prefab = ResourceManager.Instance.Load<GameObject>(AssetPaths.DamageText);
            if (prefab != null)
            {
                PoolManager.Instance.RegisterPool(PoolKey.DamageText, prefab, 20);
            }
            else
            {
                Debug.LogWarning("[GameSceneEntry] DamageText prefab not found at UI/DamageText");
            }
        }
    }
    
    
    private Vector3 GetSpawnPosition()
    {
        PlayerData playerData = GamePlayerDataService.Instance.GetCurrentPlayerData();

        if (playerData != null &&
            playerData.runtimeData != null &&
            playerData.runtimeData.hasValidPosition)
        {
            Vector3 savedPos = new Vector3(
                playerData.runtimeData.posX,
                playerData.runtimeData.posY,
                playerData.runtimeData.posZ
            );

            Debug.Log("[GameSceneEntry] 使用存档位置生成玩家：" + savedPos);
            return savedPos;
        }

        Debug.Log("[GameSceneEntry] 使用默认出生点");
        return playerSpawnPoint != null ? playerSpawnPoint.position : Vector3.zero;
    }

    private Quaternion GetSpawnRotation()
    {
        PlayerData playerData = GamePlayerDataService.Instance.GetCurrentPlayerData();

        if (playerData != null &&
            playerData.runtimeData != null &&
            playerData.runtimeData.hasValidPosition)
        {
            return Quaternion.Euler(0f, playerData.runtimeData.rotY, 0f);
        }

        return playerSpawnPoint != null ? playerSpawnPoint.rotation : Quaternion.identity;
    }

    public void SaveCurrentPlayerTransform()
    {
        if (playerInstance == null)
        {
            Debug.LogWarning("[GameSceneEntry] SaveCurrentPlayerTransform failed, playerInstance null");
            return;
        }

        var entity = playerInstance.GetComponent<PlayerEntity>();
        if (entity == null)
        {
            Debug.LogWarning("[GameSceneEntry] SaveCurrentPlayerTransform failed, PlayerEntity missing.");
            return;
        }
        playerSaveService.SaveCurrentPlayer(entity);
    }

    private void OpenMainPanel()
    {
        Debug.Log("[GameSceneEntry] OpenMainPanel");
        UIManager.Instance.ShowMainPage<MainPanel>(true, false);
    }

    private void CleanupOnFail()
    {
        if (playerInstance != null)
        {
            Destroy(playerInstance);
            playerInstance = null;
        }
        if (mainCameraInstance != null)
        {
            Destroy(mainCameraInstance);
            mainCameraInstance = null;
        }
        if (followCameraInstance != null)
        {
            Destroy(followCameraInstance);
            followCameraInstance = null;
        }
        MiniMapService.Instance.Clear();
        PlayerLocator.Instance.Clear();
        MonsterRuntimeRegistry.Instance.Clear();
    }

    private void OnDestroy()
    {
        MonsterRuntimeRegistry.Instance.Clear();
    }
}
