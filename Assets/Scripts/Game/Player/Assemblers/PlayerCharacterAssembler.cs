using UnityEngine;

public static class PlayerCharacterAssembler
{
    public static bool TryAssemble(
        PlayerData playerData,
        Vector3 spawnPos,
        Quaternion spawnRot,
        out GameObject playerInstance,
        out ThirdPersonController controller,
        out PlayerEntity entity,
        out PlayerNavigator navigator)
    {
        playerInstance = null;
        controller = null;
        entity = null;
        navigator = null;

        if (playerData == null)
        {
            Debug.LogError("[PlayerCharacterAssembler] playerData is null.");
            return false;
        }

        GameObject playerPrefab = ResourceManager.Instance.Load<GameObject>(AssetPaths.PlayerArmature);
        if (playerPrefab == null)
        {
            Debug.LogError("[PlayerCharacterAssembler] player prefab not found.");
            return false;
        }

        playerInstance = Object.Instantiate(playerPrefab, spawnPos, spawnRot);

        controller = playerInstance.GetComponent<ThirdPersonController>();
        if (controller == null)
        {
            Debug.LogError("[PlayerCharacterAssembler] ThirdPersonController missing.");
            return false;
        }

        CharacterController cc = playerInstance.GetComponent<CharacterController>();
        if (cc == null)
        {
            Debug.LogError("[PlayerCharacterAssembler] CharacterController missing.");
            return false;
        }

        Transform cameraRoot = playerInstance.transform.Find(ObjectNames.PlayerCameraRoot);
        if (cameraRoot == null)
        {
            Debug.LogError("[PlayerCharacterAssembler] cameraRoot missing.");
            return false;
        }

        Transform modelRoot = playerInstance.transform.Find(ObjectNames.ModelRoot);
        if (modelRoot == null)
        {
            Debug.LogError("[PlayerCharacterAssembler] modelRoot missing.");
            return false;
        }

        RoleVisualSetting visualSetting = PlayerVisualConfig.GetRoleVisualSetting(playerData.baseData.classId);
        string visualPath = visualSetting.path;
        if (string.IsNullOrEmpty(visualPath))
        {
            Debug.LogError("[PlayerCharacterAssembler] visualPath is empty.");
            return false;
        }

        GameObject visualPrefab = ResourceManager.Instance.Load<GameObject>(visualPath);
        if (visualPrefab == null)
        {
            Debug.LogError($"[PlayerCharacterAssembler] visual prefab not found: {visualPath}");
            return false;
        }

        for (int i = modelRoot.childCount - 1; i >= 0; i--)
        {
            Object.Destroy(modelRoot.GetChild(i).gameObject);
        }

        GameObject visualInstance = Object.Instantiate(visualPrefab, modelRoot);
        visualInstance.name = visualPrefab.name;
        visualInstance.transform.localPosition = visualSetting.localPosition;
        visualInstance.transform.localRotation = Quaternion.Euler(visualSetting.localRotation);
        visualInstance.transform.localScale = visualSetting.localScale;

        entity = playerInstance.GetComponent<PlayerEntity>();
        if (entity == null) entity = playerInstance.AddComponent<PlayerEntity>();

        var inputProxy = playerInstance.GetComponent<PlayerInputProxy>();
        if (inputProxy == null) inputProxy = playerInstance.AddComponent<PlayerInputProxy>();

        var locomotionBrain = playerInstance.GetComponent<PlayerLocomotionBrain>();
        if (locomotionBrain == null) locomotionBrain = playerInstance.AddComponent<PlayerLocomotionBrain>();

        var staminaSystem = playerInstance.GetComponent<PlayerStaminaSystem>();
        if (staminaSystem == null) staminaSystem = playerInstance.AddComponent<PlayerStaminaSystem>();

        navigator = playerInstance.GetComponent<PlayerNavigator>();
        if (navigator == null) navigator = playerInstance.AddComponent<PlayerNavigator>();

        controller.InitCameraReference();
        return true;
    }
}
