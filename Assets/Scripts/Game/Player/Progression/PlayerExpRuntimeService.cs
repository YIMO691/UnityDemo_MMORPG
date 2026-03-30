using UnityEngine;

public static class PlayerExpRuntimeService
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
        if (evt == null) return;

        var comp = evt.deadEntity as Component;
        if (comp == null) return;

        var monster = comp.GetComponent<MonsterEntity>();
        if (monster == null) return;

        if (evt.killer == null) return;
        var killerComp = evt.killer as Component;
        if (killerComp == null) return;
        var player = killerComp.GetComponent<PlayerEntity>();
        if (player == null) return;

        var cfg = monster.Config;
        if (cfg == null) return;

        int exp = cfg.expReward;
        if (exp <= 0) return;

        PlayerProgressionService.Instance.AddExpToCurrentPlayer(exp);
        Debug.Log("[ExpRuntime] Monster killed -> +" + exp + " EXP");
    }
}
