using System.Collections.Generic;
using UnityEngine;

public static class InventoryActionService
{
    public static bool TryUseItem(ItemDetailContext context)
    {
        if (context == null)
        {
            Debug.LogWarning("[InventoryActionService] TryUseItem failed: context is null.");
            return false;
        }
        if (context.isEmpty)
        {
            Debug.LogWarning("[InventoryActionService] TryUseItem failed: empty slot.");
            return false;
        }

        PlayerData playerData = GamePlayerDataService.Instance.GetCurrentPlayerData();
        if (playerData == null || playerData.inventoryData == null || playerData.inventoryData.slots == null)
        {
            Debug.LogWarning("[InventoryActionService] TryUseItem failed: player inventory is null.");
            return false;
        }

        ItemConfig cfg = ItemConfigManager.Instance.GetConfig(context.itemId);
        if (cfg == null)
        {
            Debug.LogWarning($"[InventoryActionService] TryUseItem failed: item config not found. itemId={context.itemId}");
            return false;
        }
        if (!cfg.canUse)
        {
            Debug.LogWarning($"[InventoryActionService] TryUseItem failed: item can not use. itemId={context.itemId}");
            return false;
        }

        InventorySlotData slotData = FindSlot(playerData.inventoryData.slots, context.slotIndex, context.itemId);
        if (slotData == null)
        {
            Debug.LogWarning($"[InventoryActionService] TryUseItem failed: slot not found. slotIndex={context.slotIndex}, itemId={context.itemId}");
            return false;
        }

        slotData.count -= 1;
        if (slotData.count <= 0)
        {
            playerData.inventoryData.slots.Remove(slotData);
            RebuildSlotIndex(playerData.inventoryData.slots);
        }

        EventBus.Publish(new InventoryChangedEvent());
        Debug.Log($"[InventoryActionService] use item success. itemId={context.itemId}, remain={Mathf.Max(0, slotData.count)}");
        return true;
    }

    public static bool TryDropItem(ItemDetailContext context)
    {
        if (context == null)
        {
            Debug.LogWarning("[InventoryActionService] TryDropItem failed: context is null.");
            return false;
        }
        if (context.isEmpty)
        {
            Debug.LogWarning("[InventoryActionService] TryDropItem failed: empty slot.");
            return false;
        }

        PlayerData playerData = GamePlayerDataService.Instance.GetCurrentPlayerData();
        if (playerData == null || playerData.inventoryData == null || playerData.inventoryData.slots == null)
        {
            Debug.LogWarning("[InventoryActionService] TryDropItem failed: player inventory is null.");
            return false;
        }

        ItemConfig cfg = ItemConfigManager.Instance.GetConfig(context.itemId);
        if (cfg == null)
        {
            Debug.LogWarning($"[InventoryActionService] TryDropItem failed: item config not found. itemId={context.itemId}");
            return false;
        }
        if (!cfg.canDrop)
        {
            Debug.LogWarning($"[InventoryActionService] TryDropItem failed: item can not drop. itemId={context.itemId}");
            return false;
        }

        InventorySlotData slotData = FindSlot(playerData.inventoryData.slots, context.slotIndex, context.itemId);
        if (slotData == null)
        {
            Debug.LogWarning($"[InventoryActionService] TryDropItem failed: slot not found. slotIndex={context.slotIndex}, itemId={context.itemId}");
            return false;
        }

        playerData.inventoryData.slots.Remove(slotData);
        RebuildSlotIndex(playerData.inventoryData.slots);

        EventBus.Publish(new InventoryChangedEvent());
        Debug.Log($"[InventoryActionService] drop item success. itemId={context.itemId}");
        return true;
    }

    private static InventorySlotData FindSlot(List<InventorySlotData> slots, int slotIndex, int itemId)
    {
        if (slots == null) return null;
        for (int i = 0; i < slots.Count; i++)
        {
            InventorySlotData slot = slots[i];
            if (slot == null) continue;
            if (slot.slotIndex != slotIndex) continue;
            if (slot.itemId != itemId) continue;
            return slot;
        }
        return null;
    }

    private static void RebuildSlotIndex(List<InventorySlotData> slots)
    {
        if (slots == null) return;
        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i] != null) slots[i].slotIndex = i;
        }
    }
}
