using System.Collections;
using UnityEngine;

public static class PlayerRespawnRuntimeService
{
    private static bool initialized;
    private static bool isRespawning;

    // 可按需要调整
    private const float RestoreControlDelay = 0.3f;

    public static void Init()
    {
        if (initialized) return;
        initialized = true;
        isRespawning = false;
    }

    public static bool IsRespawning => isRespawning;

    public static void RespawnNow(PlayerEntity player)
    {
        if (player == null)
        {
            Debug.LogWarning("[Respawn] RespawnNow failed: player is null");
            return;
        }

        if (isRespawning)
        {
            Debug.LogWarning("[Respawn] RespawnNow ignored: already respawning");
            return;
        }

        isRespawning = true;

        Vector3 spawnPos = PlayerSpawnPoint.GetSpawnPosition();

        // 1. 先锁控制，避免这一帧/下一帧继续跑旧输入
        var brain = player.GetComponent<PlayerLocomotionBrain>();
        if (brain != null)
        {
            brain.SetFullControlEnabled(false);
        }

        // 2. 停导航，避免导航继续往旧目标点写移动命令
        var nav = player.GetComponent<PlayerNavigator>();
        if (nav != null)
        {
            nav.StopNavigation();
        }

        // 3. 真正复活
        player.ReviveFull();

        // 4. 传送到出生点
        player.TeleportTo(spawnPos);

        // 5. 同步存档快照，避免后续又被旧位置覆盖
        player.CaptureRuntimeSnapshot();

        // 6. 清死亡去重标记，允许下次再次死亡
        DeathRuntimeService.ClearProcessed(player);

        // 7. 关闭复活遮罩
        RespawnOverlayRuntime.Instance.Hide();

        // 8. 稍后恢复控制
        CoroutineRunner.Instance.StartCoroutine(RestoreControlLater(player));

        Debug.Log("[Respawn] Done");
    }

    private static IEnumerator RestoreControlLater(PlayerEntity player)
    {
        yield return new WaitForSeconds(RestoreControlDelay);

        if (player == null)
        {
            isRespawning = false;
            yield break;
        }

        var brain = player.GetComponent<PlayerLocomotionBrain>();
        if (brain != null)
        {
            brain.SetFullControlEnabled(true);
        }

        isRespawning = false;
    }

    private static IEnumerator CheckRespawnPositionNextFrame(PlayerEntity player) { yield return null; }
}
