using UnityEngine;

public static class LootRuntimeService
{
    private static bool initialized;

    public static void Init()
    {
        if (initialized) return;
        initialized = true;
        EventBus.Subscribe<DeathEvent>(OnDeath);
    }

    private static void OnDeath(DeathEvent evt)
    {
        if (evt == null || evt.deadEntity == null) return;
        var comp = evt.deadEntity as Component;
        if (comp == null) return;
        var lootProvider = comp as ILootProvider;
        if (lootProvider == null) return;
        lootProvider.DropLoot(evt.killer);
    }

    public static void HandleMonsterLoot(MonsterEntity monster, ICombatSource killer)
    {
        if (monster == null)
        {
            Debug.LogWarning("[LootRuntimeService] monster is null.");
            return;
        }
        if (monster.Config == null)
        {
            Debug.LogWarning("[LootRuntimeService] monster.Config is null.");
            return;
        }
        if (monster.Config.dropTableId <= 0)
        {
            Debug.Log($"[LootRuntimeService] monster={monster.DisplayName} has no dropTableId, skip loot.");
            return;
        }
        var drops = LootResolver.Resolve(monster.Config.dropTableId);
        if (drops == null || drops.Count == 0)
        {
            Debug.Log($"[LootRuntimeService] no drops resolved. monster={monster.DisplayName}, dropTableId={monster.Config.dropTableId}");
            return;
        }
        var playerData = GamePlayerDataService.Instance.GetCurrentPlayerData();
        if (playerData == null)
        {
            Debug.LogWarning("[LootRuntimeService] current player data is null.");
        }
        for (int i = 0; i < drops.Count; i++)
        {
            var drop = drops[i];
            bool added = playerData != null && InventoryService.TryAddItem(playerData, drop.itemId, drop.count);
            Debug.Log($"[LootRuntimeService] drop resolved. monster={monster.DisplayName}, itemId={drop.itemId}, count={drop.count}, addToInventory={added}");
        }
    }
}
