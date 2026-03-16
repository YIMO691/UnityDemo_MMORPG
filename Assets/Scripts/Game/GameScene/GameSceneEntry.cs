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
            return;
        }

        CreateCamera();
        CreateAnimator();   // 你现在想写在这里也行，本质是创建 PlayerArmature
        BindCamera();
        OpenMainPanel();

        Debug.Log("[GameSceneEntry] InitScene end");
    }


    private void CreateAnimator()
    {
        Debug.Log("[GameSceneEntry] CreateAnimator begin");

        GameObject playerPrefab = Resources.Load<GameObject>(UIPaths.PlayerArmature);
        if (playerPrefab == null)
        {
            Debug.LogError("[GameSceneEntry] PlayerArmature prefab not found.");
            return;
        }

        Vector3 spawnPos = GetSpawnPosition();
        Quaternion spawnRot = GetSpawnRotation();

        playerInstance = Instantiate(playerPrefab, spawnPos, spawnRot);
        playerInstance.name = $"Player_{GameRuntime.CurrentPlayerData.baseData.roleName}_{GameRuntime.CurrentPlayerData.baseData.classId}";

        Debug.Log($"[GameSceneEntry] PlayerArmature created: {playerInstance.name}");

        // 检查关键组件
        ThirdPersonController controller = playerInstance.GetComponent<ThirdPersonController>();
        if (controller == null)
        {
            Debug.LogError("[GameSceneEntry] ThirdPersonController not found on PlayerArmature.");
            return;
        }

        CharacterController characterController = playerInstance.GetComponent<CharacterController>();
        if (characterController == null)
        {
            Debug.LogError("[GameSceneEntry] CharacterController not found on PlayerArmature.");
            return;
        }

        Transform cameraRoot = playerInstance.transform.Find("PlayerCameraRoot");
        if (cameraRoot == null)
        {
            Debug.LogError("[GameSceneEntry] PlayerCameraRoot not found.");
            return;
        }

        Transform modelRoot = playerInstance.transform.Find("ModelRoot");
        if (modelRoot == null)
        {
            Debug.LogError("[GameSceneEntry] ModelRoot not found.");
            return;
        }

        // 把职业模型挂进去
        AttachRoleVisual(modelRoot, GameRuntime.CurrentPlayerData.baseData.classId);

        // 强制重新找主相机，避免 ThirdPersonController Awake 早于相机创建
        controller.InitCameraReference();

        Debug.Log("[GameSceneEntry] CreateAnimator end");
    }
    private string GetRoleVisualPath(int classId)
    {
        switch (classId)
        {
            case 1:
                return UIPaths.RoleEngineer;
            case 2:
                return UIPaths.RoleInfantry;
            case 3:
                return UIPaths.RoleMedic;
            case 4:
                return UIPaths.RoleSniper;
            default:
                return null;
        }
    }

    private void AttachRoleVisual(Transform modelRoot, int classId)
    {
        string visualPath = GetRoleVisualPath(classId);
        if (string.IsNullOrEmpty(visualPath))
        {
            Debug.LogError($"[GameSceneEntry] Invalid role visual path for classId: {classId}");
            return;
        }

        GameObject visualPrefab = Resources.Load<GameObject>(visualPath);
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


    private void CreateCamera()
    {
        Debug.Log("[GameSceneEntry] CreateCamera begin");

        GameObject mainCameraPrefab = Resources.Load<GameObject>(UIPaths.MainCamera);
        GameObject followCameraPrefab = Resources.Load<GameObject>(UIPaths.PlayerFollowCamera);

        Debug.Log($"[GameSceneEntry] mainCameraPrefab = {(mainCameraPrefab == null ? "null" : mainCameraPrefab.name)}");
        Debug.Log($"[GameSceneEntry] followCameraPrefab = {(followCameraPrefab == null ? "null" : followCameraPrefab.name)}");

        if (mainCameraPrefab == null)
        {
            Debug.LogError("[GameSceneEntry] MainCamera prefab not found.");
            return;
        }

        if (followCameraPrefab == null)
        {
            Debug.LogError("[GameSceneEntry] PlayerFollowCamera prefab not found.");
            return;
        }

        mainCameraInstance = Instantiate(mainCameraPrefab);
        followCameraInstance = Instantiate(followCameraPrefab);

        Debug.Log("[GameSceneEntry] Camera created");
    }

    private void BindCamera()
    {
        Debug.Log("[GameSceneEntry] BindCamera begin");

        if (playerInstance == null || followCameraInstance == null)
        {
            Debug.LogError("[GameSceneEntry] BindCamera failed, missing player or follow camera.");
            return;
        }

        ThirdPersonController controller = playerInstance.GetComponent<ThirdPersonController>();
        if (controller == null)
        {
            Debug.LogError("[GameSceneEntry] ThirdPersonController not found.");
            return;
        }

        CinemachineVirtualCamera virtualCamera = followCameraInstance.GetComponent<CinemachineVirtualCamera>();
        if (virtualCamera == null)
        {
            Debug.LogError("[GameSceneEntry] CinemachineVirtualCamera not found.");
            return;
        }

        if (controller.CinemachineCameraTarget == null)
        {
            Debug.LogError("[GameSceneEntry] CinemachineCameraTarget is null.");
            return;
        }

        virtualCamera.Follow = controller.CinemachineCameraTarget.transform;
        virtualCamera.LookAt = controller.CinemachineCameraTarget.transform;

        Debug.Log("[GameSceneEntry] BindCamera success");
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
}
