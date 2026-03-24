
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
            nav.SetAgentId("Player");
        }

        Debug.Log("[GameSceneEntry] InitScene end");
    }

    private bool TryAssembleScene(out ThirdPersonController controller)
    {
        controller = null;
        bool okCreate = CameraRigAssembler.TryCreate(out mainCameraInstance, out followCameraInstance);
        bool okAssemble = PlayerCharacterAssembler.TryAssemble(GamePlayerDataService.Instance.GetCurrentPlayerData(), GetSpawnPosition(), GetSpawnRotation(), out playerInstance, out controller);
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
        PlayerLocator.Instance.Register(playerInstance.transform);
        var data = GamePlayerDataService.Instance.GetCurrentPlayerData();
        RuntimeSceneCommitter.WriteSceneContext(data, playerInstance.transform);
        NavigationAgentAssembler.EnsurePlayerNavigator(playerInstance, NavigationConsts.PlayerAgentId);
        EnsureDebugCanvas();
        RestoreMonstersIfAny();
        InitMonsterModule();
    }

    private void EnsureDebugCanvas()
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        GameObject existing = GameObject.Find("[DebugCanvas]");
        if (existing != null) return;

        GameObject prefab = ResourceManager.Instance.Load<GameObject>(AssetPaths.DebugCanvas);
        if (prefab == null)
        {
            Debug.LogWarning("[GameSceneEntry] DebugCanvas prefab not found: UI/Root/DebugCanvas");
            return;
        }
        GameObject go = Instantiate(prefab);
        go.name = "[DebugCanvas]";
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

    private bool TryCreatePlayerCharacter() { return false; }
    private string GetRoleVisualPath(int classId) { return null; }
    private bool ValidatePlayerComponents(GameObject player) { return false; }
    private void AttachRoleVisual(Transform modelRoot, int classId) { }

    private void InitMonsterModule()
    {
        var spawnPoints = FindObjectsOfType<MonsterSpawnPoint>();
        Debug.Log($"[Monster] SpawnPoints found: {spawnPoints.Length}");
        for (int i = 0; i < spawnPoints.Length; i++)
        {
            spawnPoints[i].Init();
        }
    }
    private void RestoreMonstersIfAny()
    {
        var data = GamePlayerDataService.Instance.GetCurrentPlayerData();
        if (data == null) return;
        if (data.monsterData == null || data.monsterData.Count == 0) return;
        var svc = new MonsterSaveService();
        svc.RestoreScene(data.monsterData);
    }
    private bool TryCreateCamera() { return false; }
    private bool TryBindCamera() { return false; }
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

        playerSaveService.SaveCurrentPlayer(playerInstance.transform);
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
