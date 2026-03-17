using Cinemachine;
using Game.Runtime;
using StarterAssets;
using UnityEngine;

public class GameSceneEntry : MonoBehaviour
{
    [SerializeField] private Transform playerSpawnPoint;

    private GameObject playerInstance;
    private GameObject mainCameraInstance;
    private GameObject followCameraInstance;
    private readonly PlayerSaveService playerSaveService = new PlayerSaveService();

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

        bool ok =
            TryCreateCamera() &&
            TryCreatePlayerCharacter() &&
            TryBindCamera();

        if (!ok)
        {
            CleanupOnFail();
            SceneNavigator.EnterBeginScene();
            return;
        }
        OpenMainPanel();

        Debug.Log("[GameSceneEntry] InitScene end");
    }

    private bool TryCreatePlayerCharacter()
    {
        Debug.Log("[GameSceneEntry] CreatePlayerCharacter begin");

        GameObject playerPrefab = ResourceManager.Instance.Load<GameObject>(AssetPaths.PlayerArmature);
        if (playerPrefab == null)
        {
            Debug.LogError("[GameSceneEntry] PlayerArmature prefab not found.");
            return false;
        }

        Vector3 spawnPos = GetSpawnPosition();
        Quaternion spawnRot = GetSpawnRotation();

        playerInstance = Instantiate(playerPrefab, spawnPos, spawnRot);
        playerInstance.name = $"Player_{GameRuntime.CurrentPlayerData.baseData.roleName}_{GameRuntime.CurrentPlayerData.baseData.classId}";

        Debug.Log($"[GameSceneEntry] PlayerArmature created: {playerInstance.name}");

        if (!ValidatePlayerComponents(playerInstance)) return false;

        // 把职业模型挂进去
        Transform modelRoot = playerInstance.transform.Find("ModelRoot");
        AttachRoleVisual(modelRoot, GameRuntime.CurrentPlayerData.baseData.classId);

        Debug.Log("[GameSceneEntry] CreatePlayerCharacter end");
        return true;
    }
    private string GetRoleVisualPath(int classId)
    {
        return RoleVisualPaths.GetPath(classId);
    }
    private bool ValidatePlayerComponents(GameObject player)
    {
        ThirdPersonController controller = player.GetComponent<ThirdPersonController>();
        if (controller == null)
        {
            Debug.LogError("[GameSceneEntry] ThirdPersonController not found on PlayerArmature.");
            return false;
        }

        CharacterController characterController = player.GetComponent<CharacterController>();
        if (characterController == null)
        {
            Debug.LogError("[GameSceneEntry] CharacterController not found on PlayerArmature.");
            return false;
        }

        Transform cameraRoot = player.transform.Find("PlayerCameraRoot");
        if (cameraRoot == null)
        {
            Debug.LogError("[GameSceneEntry] PlayerCameraRoot not found.");
            return false;
        }

        Transform modelRoot = player.transform.Find("ModelRoot");
        if (modelRoot == null)
        {
            Debug.LogError("[GameSceneEntry] ModelRoot not found.");
            return false;
        }

        // 强制重新找主相机，避免 ThirdPersonController Awake 早于相机创建
        controller.InitCameraReference();
        return true;
    }

    private void AttachRoleVisual(Transform modelRoot, int classId)
    {
        string visualPath = GetRoleVisualPath(classId);
        if (string.IsNullOrEmpty(visualPath))
        {
            Debug.LogError($"[GameSceneEntry] Invalid role visual path for classId: {classId}");
            return;
        }

        GameObject visualPrefab = ResourceManager.Instance.Load<GameObject>(visualPath);
        if (visualPrefab == null)
        {
            Debug.LogError($"[GameSceneEntry] Role visual prefab not found: {visualPath}");
            return;
        }

        // 清空旧模型
        for (int i = modelRoot.childCount - 1; i >= 0; i--)
        {
            Destroy(modelRoot.GetChild(i).gameObject);
        }

        GameObject visualInstance = Instantiate(visualPrefab, modelRoot);
        visualInstance.name = visualPrefab.name;
        visualInstance.transform.localPosition = Vector3.zero;
        visualInstance.transform.localRotation = Quaternion.identity;
        visualInstance.transform.localScale = Vector3.one;

        Debug.Log($"[GameSceneEntry] AttachRoleVisual success: {visualInstance.name}");
    }


    private bool TryCreateCamera()
    {
        Debug.Log("[GameSceneEntry] CreateCamera begin");

        GameObject mainCameraPrefab = ResourceManager.Instance.Load<GameObject>(AssetPaths.MainCamera);
        GameObject followCameraPrefab = ResourceManager.Instance.Load<GameObject>(AssetPaths.PlayerFollowCamera);

        Debug.Log($"[GameSceneEntry] mainCameraPrefab = {(mainCameraPrefab == null ? "null" : mainCameraPrefab.name)}");
        Debug.Log($"[GameSceneEntry] followCameraPrefab = {(followCameraPrefab == null ? "null" : followCameraPrefab.name)}");

        if (mainCameraPrefab == null)
        {
            Debug.LogError("[GameSceneEntry] MainCamera prefab not found.");
            return false;
        }

        if (followCameraPrefab == null)
        {
            Debug.LogError("[GameSceneEntry] PlayerFollowCamera prefab not found.");
            return false;
        }

        mainCameraInstance = Instantiate(mainCameraPrefab);
        followCameraInstance = Instantiate(followCameraPrefab);

        Debug.Log("[GameSceneEntry] Camera created");
        return true;
    }

    private bool TryBindCamera()
    {
        Debug.Log("[GameSceneEntry] BindCamera begin");

        if (playerInstance == null || followCameraInstance == null)
        {
            Debug.LogError("[GameSceneEntry] BindCamera failed, missing player or follow camera.");
            return false;
        }

        ThirdPersonController controller = playerInstance.GetComponent<ThirdPersonController>();
        if (controller == null)
        {
            Debug.LogError("[GameSceneEntry] ThirdPersonController not found.");
            return false;
        }

        CinemachineVirtualCamera virtualCamera = followCameraInstance.GetComponent<CinemachineVirtualCamera>();
        if (virtualCamera == null)
        {
            Debug.LogError("[GameSceneEntry] CinemachineVirtualCamera not found.");
            return false;
        }

        if (controller.CinemachineCameraTarget == null)
        {
            Debug.LogError("[GameSceneEntry] CinemachineCameraTarget is null.");
            return false;
        }

        virtualCamera.Follow = controller.CinemachineCameraTarget.transform;
        virtualCamera.LookAt = controller.CinemachineCameraTarget.transform;

        Debug.Log("[GameSceneEntry] BindCamera success");
        return true;
    }
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
    }
}
