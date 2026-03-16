using UnityEngine;

public static class PlayerVisualAssembler
{
    public static void AttachRoleVisual(GameObject playerRoot, PlayerData playerData)
    {
        if (playerRoot == null)
        {
            Debug.LogError("[PlayerVisualAssembler] playerRoot is null.");
            return;
        }

        if (playerData == null)
        {
            Debug.LogError("[PlayerVisualAssembler] playerData is null.");
            return;
        }

        Transform modelRoot = playerRoot.transform.Find("ModelRoot");
        if (modelRoot == null)
        {
            Debug.LogError("[PlayerVisualAssembler] ModelRoot not found.");
            return;
        }

        ClearChildren(modelRoot);

        RoleVisualSetting visual = PlayerVisualConfig.GetRoleVisualSetting(playerData.baseData.classId);
        string visualPath = visual.path;
        if (string.IsNullOrEmpty(visualPath))
        {
            Debug.LogError($"[PlayerVisualAssembler] Invalid visual path for classId: {playerData.baseData.classId}");
            return;
        }

        GameObject visualPrefab = Resources.Load<GameObject>(visualPath);
        if (visualPrefab == null)
        {
            Debug.LogError($"[PlayerVisualAssembler] Visual prefab not found: {visualPath}");
            return;
        }

        GameObject visualInstance = Object.Instantiate(visualPrefab, modelRoot);
        visualInstance.name = visualPrefab.name;

        visualInstance.transform.localPosition = Vector3.zero;
        visualInstance.transform.localRotation = Quaternion.identity;
        visualInstance.transform.localScale = Vector3.one;

        Debug.Log($"[PlayerVisualAssembler] AttachRoleVisual success: {visualInstance.name}");
    }

    private static void ClearChildren(Transform parent)
    {
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            Object.Destroy(parent.GetChild(i).gameObject);
        }
    }
}
