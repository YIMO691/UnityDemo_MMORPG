using UnityEngine;

public class PlayerSaveService
{
    private readonly RuntimeSaveService runtimeSaveService = new RuntimeSaveService();

    public void SaveCurrentPlayer(Transform playerTransform)
    {
        PlayerData playerData = Game.Runtime.GameRuntime.CurrentPlayerData;
        if (playerData == null)
        {
            playerData = DataManager.Instance.GetCurrentPlayerData();
        }

        if (playerData == null)
        {
            Debug.LogWarning("[PlayerSaveService] Save failed, current player data is null.");
            return;
        }

        runtimeSaveService.SavePlayerTransform(playerTransform, playerData);

        DataManager.Instance.SetCurrentPlayerData(playerData);

        int slotId = DataManager.Instance.GetCurrentSlotId();
        if (slotId < 0)
        {
            Debug.LogWarning("[PlayerSaveService] Save failed, current slot id invalid.");
            return;
        }

        DataManager.Instance.SaveCurrentPlayerDataToSlot(slotId);

        Debug.Log("[PlayerSaveService] SaveCurrentPlayer success. slotId=" + slotId);
    }
}
