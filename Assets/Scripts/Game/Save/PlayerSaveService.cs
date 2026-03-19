using UnityEngine;

public class PlayerSaveService
{
    private readonly RuntimeSaveService runtimeSaveService = new RuntimeSaveService();

    public void SaveCurrentPlayer(Transform playerTransform)
    {
        PlayerData playerData = Game.Runtime.GameRuntime.CurrentPlayerData ?? GamePlayerDataService.Instance.GetCurrentPlayerData();

        if (playerData == null)
        {
            Debug.LogWarning("[PlayerSaveService] Save failed, current player data is null.");
            return;
        }

        runtimeSaveService.SavePlayerTransform(playerTransform, playerData);

        GamePlayerDataService.Instance.SetCurrentPlayerData(playerData);

        int slotId = DataManager.Instance.GetCurrentSlotId();
        if (slotId < 0)
        {
            Debug.LogWarning("[PlayerSaveService] Save failed, current slot id invalid.");
            return;
        }

        GamePlayerDataService.Instance.SaveCurrentPlayerDataToSlot(slotId);

        Debug.Log("[PlayerSaveService] SaveCurrentPlayer success. slotId=" + slotId);
    }
}
