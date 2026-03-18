using Cinemachine;
using Game.Runtime;
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

        if (GameRuntime.CurrentPlayerData == null)
        {
            Debug.LogError("[GameSceneEntry] CurrentPlayerData is null.");
            SceneNavigator.EnterBeginScene();
            return;
        }

        bool okCreate = CameraRigAssembler.TryCreate(out mainCameraInstance, out followCameraInstance);
        bool okAssemble = PlayerCharacterAssembler.TryAssemble(GameRuntime.CurrentPlayerData, GetSpawnPosition(), GetSpawnRotation(), out playerInstance, out var controller);
        bool okBind = okAssemble && followCameraInstance != null && CameraRigAssembler.TryBind(controller, followCameraInstance);

        if (!okBind && okCreate && okAssemble)
        {
            CameraRigAssembler.FallbackBind(controller, mainCameraInstance);
            okBind = true;
        }

        if (!(okCreate && okAssemble && okBind))
        {
            CleanupOnFail();
            SceneNavigator.EnterBeginScene();
            return;
        }

        InitMiniMap();
        OpenMainPanel();

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

    private void InitMiniMap()
    {
        if (miniMapCamera == null)
        {
            GameObject go = GameObject.Find("MiniMapCamera");
            if (go != null)
            {
                miniMapCamera = go.GetComponent<Camera>();
            }
        }

        if (miniMapCamera == null)
        {
            Debug.LogWarning("[GameSceneEntry] miniMapCamera 未找到。");
            return;
        }

        if (miniMapController == null)
        {
            miniMapController = miniMapCamera.GetComponent<MiniMapCameraController>();
        }

        if (miniMapController == null)
        {
            Debug.LogWarning("[GameSceneEntry] MiniMapCameraController 缺失。");
            return;
        }

        RenderTexture rt = ResourceManager.Instance.Load<RenderTexture>(AssetPaths.MiniMapRenderTexture);
        if (rt == null)
        {
            Debug.LogWarning("[GameSceneEntry] 小地图 RenderTexture 加载失败: " + AssetPaths.MiniMapRenderTexture);
            return;
        }

        miniMapCamera.targetTexture = rt;

        MiniMapService.Instance.Register(miniMapCamera, rt, miniMapController);

        if (playerInstance == null)
        {
            Debug.LogWarning("[GameSceneEntry] playerInstance 为空，无法绑定 MiniMap target。");
            return;
        }

        Transform target = playerInstance.transform;
        if (target == null)
        {
            Debug.LogWarning("[GameSceneEntry] PlayerCameraRoot 未找到，改为绑定玩家根节点。");
            target = playerInstance.transform;
        }

        miniMapController.SetTarget(target);
        MiniMapService.Instance.BindTarget(target);

        Debug.Log("[GameSceneEntry] MiniMap 初始化完成，目标 = " + target.name);
    }



    private bool TryCreatePlayerCharacter() { return false; }
    private string GetRoleVisualPath(int classId) { return null; }
    private bool ValidatePlayerComponents(GameObject player) { return false; }
    private void AttachRoleVisual(Transform modelRoot, int classId) { }
    private bool TryCreateCamera() { return false; }
    private bool TryBindCamera() { return false; }
    private Vector3 GetSpawnPosition()
    {
        PlayerData playerData = GameRuntime.CurrentPlayerData;

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
        PlayerData playerData = GameRuntime.CurrentPlayerData;

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
    }
}
