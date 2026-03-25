using UnityEngine;

public class PlayerSaveService
{
    private readonly RuntimeSaveService runtimeSaveService = new RuntimeSaveService();

    public void SaveCurrentPlayer(PlayerEntity playerEntity)
    {
        if (playerEntity == null)
        {
            Debug.LogWarning("[PlayerSaveService] Save failed, playerEntity is null.");
            return;
        }
        PlayerData playerData = playerEntity.Data;

        if (playerData == null)
        {
            Debug.LogWarning("[PlayerSaveService] Save failed, current player data is null.");
            return;
        }

        runtimeSaveService.SavePlayerRuntime(playerEntity);
        runtimeSaveService.SaveMonsters(playerData);

        int slotId = GamePlayerDataService.Instance.GetCurrentSlotId();
        if (slotId < 1)
        {
            Debug.LogWarning("[PlayerSaveService] Save failed, current slot id invalid.");
            return;
        }

        GamePlayerDataService.Instance.SavePlayerDataToSlot(slotId, playerData);

        Debug.Log("[PlayerSaveService] SaveCurrentPlayer success. slotId=" + slotId);
    }
}
