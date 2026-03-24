using UnityEngine;

public class PlayerSaveService
{
    private readonly RuntimeSaveService runtimeSaveService = new RuntimeSaveService();

    public void SaveCurrentPlayer(Transform playerTransform)
    {
        PlayerData playerData = GamePlayerDataService.Instance.GetCurrentPlayerData();

        if (playerData == null)
        {
            Debug.LogWarning("[PlayerSaveService] Save failed, current player data is null.");
            return;
        }

        runtimeSaveService.SavePlayerTransform(playerTransform, playerData);
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
