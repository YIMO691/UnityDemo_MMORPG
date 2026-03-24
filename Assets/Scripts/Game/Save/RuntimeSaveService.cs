using UnityEngine;

public class RuntimeSaveService
{
    public void SavePlayerTransform(Transform playerTransform, PlayerData playerData)
    {
        if (playerTransform == null)
        {
            Debug.LogWarning("[RuntimeSaveService] playerTransform is null.");
            return;
        }

        if (playerData == null)
        {
            Debug.LogWarning("[RuntimeSaveService] playerData is null.");
            return;
        }

        if (playerData.runtimeData == null)
        {
            playerData.runtimeData = new PlayerRuntimeData();
        }

        Vector3 pos = playerTransform.position;
        Vector3 rot = playerTransform.eulerAngles;

        playerData.runtimeData.hasValidPosition = true;
        playerData.runtimeData.posX = pos.x;
        playerData.runtimeData.posY = pos.y;
        playerData.runtimeData.posZ = pos.z;
        playerData.runtimeData.rotY = rot.y;
        playerData.runtimeData.Scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

        Debug.Log($"[RuntimeSaveService] SavePlayerTransform success. pos={pos}, rotY={rot.y}");
    }

    public void SaveMonsters(PlayerData playerData)
    {
        if (playerData == null) return;
        var service = new MonsterSaveService();
        playerData.monsterData = service.CaptureScene();
    }
}
