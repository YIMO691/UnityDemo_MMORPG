using UnityEngine;

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

        Transform cameraRoot = playerInstance.transform.Find(ObjectNames.PlayerCameraRoot);
        if (cameraRoot == null) return false;

        Transform modelRoot = playerInstance.transform.Find(ObjectNames.ModelRoot);
        if (modelRoot == null) return false;

        RoleVisualSetting visualSetting = PlayerVisualConfig.GetRoleVisualSetting(playerData.baseData.classId);
        string visualPath = visualSetting.path;
        if (string.IsNullOrEmpty(visualPath)) return false;

        GameObject visualPrefab = ResourceManager.Instance.Load<GameObject>(visualPath);
        if (visualPrefab == null) return false;

        for (int i = modelRoot.childCount - 1; i >= 0; i--)
        {
            Object.Destroy(modelRoot.GetChild(i).gameObject);
        }

        GameObject visualInstance = Object.Instantiate(visualPrefab, modelRoot);
        visualInstance.name = visualPrefab.name;
        visualInstance.transform.localPosition = visualSetting.localPosition;
        visualInstance.transform.localRotation = Quaternion.Euler(visualSetting.localRotation);
        visualInstance.transform.localScale = visualSetting.localScale;

        controller.InitCameraReference();
        return true;
    }
}
