using System.Collections;
using UnityEngine;

public static class PlayerDeathUIController
{
    private static bool inited;

    public static void Init()
    {
        if (inited) return;
        EventBus.Subscribe<PlayerDeadEvent>(OnPlayerDead);
        inited = true;
    }

    private static void OnPlayerDead(PlayerDeadEvent evt)
    {
        var player = evt != null ? evt.player : null;
        var brain = player != null ? player.GetComponent<PlayerLocomotionBrain>() : null;
        if (brain != null) brain.SetFullControlEnabled(false);

        global::RespawnOverlayRuntime.Instance.Show(10f, () =>
        {
            PlayerRespawnRuntimeService.RespawnNow(player);
        });
    }

    private static IEnumerator AutoRespawn(PlayerEntity player) { yield return null; }
}
