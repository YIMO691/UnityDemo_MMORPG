using UnityEngine;

public class RuntimeSaveService
{
    public void SavePlayerRuntime(PlayerEntity playerEntity)
    {
        if (playerEntity == null)
        {
            Debug.LogWarning("[RuntimeSaveService] playerEntity is null.");
            return;
        }
        playerEntity.CaptureRuntimeSnapshot();
        Debug.Log("[RuntimeSaveService] SavePlayerRuntime success.");
    }

    public void SaveMonsters(PlayerData playerData)
    {
        if (playerData == null) return;
        var service = new MonsterSaveService();
        playerData.monsterData = service.CaptureScene();
    }
}
