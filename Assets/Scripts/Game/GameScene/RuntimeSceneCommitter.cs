using UnityEngine;

public static class RuntimeSceneCommitter
{
    public static void WriteSceneContext(PlayerData data, Transform playerTransform)
    {
        if (data == null || data.runtimeData == null || playerTransform == null)
        {
            Debug.LogWarning("[RuntimeSceneCommitter] 写入运行态上下文失败：参数为空。");
            return;
        }

        var active = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        data.runtimeData.Scene = active;
        var p = playerTransform.position;
        data.runtimeData.posX = p.x;
        data.runtimeData.posY = p.y;
        data.runtimeData.posZ = p.z;
        data.runtimeData.hasValidPosition = true;
    }
}
