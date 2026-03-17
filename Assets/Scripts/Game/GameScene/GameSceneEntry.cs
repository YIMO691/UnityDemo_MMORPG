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
            CameraRigAssembler.TryCreate(out mainCameraInstance, out followCameraInstance) &&
            PlayerCharacterAssembler.TryAssemble(GameRuntime.CurrentPlayerData, GetSpawnPosition(), GetSpawnRotation(), out playerInstance, out var controller) &&
            CameraRigAssembler.TryBind(controller, followCameraInstance);

        if (!ok)
        {
            CleanupOnFail();
            SceneNavigator.EnterBeginScene();
            return;
        }
        OpenMainPanel();

        Debug.Log("[GameSceneEntry] InitScene end");
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
    }
}
