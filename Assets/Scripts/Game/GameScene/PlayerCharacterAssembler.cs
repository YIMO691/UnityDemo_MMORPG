using UnityEngine;
using StarterAssets;

public static class PlayerCharacterAssembler
{
    public static bool TryAssemble(PlayerData playerData, Vector3 spawnPos, Quaternion spawnRot, out GameObject playerInstance, out ThirdPersonController controller)
    {
        playerInstance = null;
        controller = null;

        GameObject playerPrefab = ResourceManager.Instance.Load<GameObject>(AssetPaths.PlayerArmature);
        if (playerPrefab == null) return false;

        playerInstance = Object.Instantiate(playerPrefab, spawnPos, spawnRot);

        controller = playerInstance.GetComponent<ThirdPersonController>();
        if (controller == null) return false;

        CharacterController cc = playerInstance.GetComponent<CharacterController>();
        if (cc == null) return false;

        Transform cameraRoot = playerInstance.transform.Find("PlayerCameraRoot");
        if (cameraRoot == null) return false;

        Transform modelRoot = playerInstance.transform.Find("ModelRoot");
        if (modelRoot == null) return false;

        string visualPath = RoleVisualPaths.GetPath(playerData.baseData.classId);
        if (string.IsNullOrEmpty(visualPath)) return false;

        GameObject visualPrefab = ResourceManager.Instance.Load<GameObject>(visualPath);
        if (visualPrefab == null) return false;

        for (int i = modelRoot.childCount - 1; i >= 0; i--)
        {
            Object.Destroy(modelRoot.GetChild(i).gameObject);
        }

        GameObject visualInstance = Object.Instantiate(visualPrefab, modelRoot);
        visualInstance.name = visualPrefab.name;
        visualInstance.transform.localPosition = Vector3.zero;
        visualInstance.transform.localRotation = Quaternion.identity;
        visualInstance.transform.localScale = Vector3.one;

        controller.InitCameraReference();
        return true;
    }
}
