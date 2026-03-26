using System.Collections.Generic;
using UnityEngine;

public static class InventoryService
{
    public static bool TryAddItem(PlayerData playerData, int itemId, int count)
    {
        if (playerData == null)
        {
            Debug.LogWarning("[InventoryService] playerData is null.");
            return false;
        }
        if (playerData.inventoryData == null)
        {
            playerData.inventoryData = new InventoryData
            {
                slotCount = 0,
                slots = new List<InventorySlotData>()
            };
        }
        if (playerData.inventoryData.slots == null)
        {
            playerData.inventoryData.slots = new List<InventorySlotData>();
        }

        var itemCfg = ItemConfigManager.Instance.GetConfig(itemId);
        if (itemCfg == null)
        {
            Debug.LogWarning($"[InventoryService] item config not found. itemId={itemId}");
            return false;
        }

        int remain = count;
        for (int i = 0; i < playerData.inventoryData.slots.Count; i++)
        {
            var slot = playerData.inventoryData.slots[i];
            if (slot.itemId != itemId) continue;
            int canAdd = Mathf.Max(0, itemCfg.maxStack - slot.count);
            if (canAdd <= 0) continue;
            int add = Mathf.Min(canAdd, remain);
            slot.count += add;
            remain -= add;
            if (remain <= 0)
            {
                Debug.Log($"[InventoryService] add success. itemId={itemId}, count={count}");
                return true;
            }
        }

        while (remain > 0)
        {
            int add = Mathf.Min(itemCfg.maxStack, remain);
            var slot = new InventorySlotData
            {
                slotIndex = playerData.inventoryData.slots.Count,
                itemId = itemId,
                count = add
            };
            playerData.inventoryData.slots.Add(slot);
            remain -= add;
        }

        Debug.Log($"[InventoryService] add success. itemId={itemId}, count={count}");
        return true;
    }
}
