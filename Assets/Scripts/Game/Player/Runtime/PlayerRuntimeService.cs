using UnityEngine;

public static class PlayerRuntimeService
{
    public static bool CreateRuntimePlayer(
        PlayerData playerData,
        Vector3 spawnPos,
        Quaternion spawnRot,
        out GameObject playerInstance,
        out ThirdPersonController controller,
        out PlayerEntity entity)
    {
        playerInstance = null;
        controller = null;
        entity = null;

        if (playerData == null)
        {
            Debug.LogError("[PlayerRuntimeService] playerData is null.");
            return false;
        }

        bool ok = PlayerCharacterAssembler.TryAssemble(
            playerData,
            spawnPos,
            spawnRot,
            out playerInstance,
            out controller,
            out entity,
            out var navigator);


        if (!ok || playerInstance == null || controller == null || entity == null)
        {
            Debug.LogError("[PlayerRuntimeService] TryAssemble failed.");
            return false;
        }

        string runtimeId = $"Player_{playerData.baseData.roleId}";
        entity.Init(playerData, runtimeId);
        entity.ApplyRuntimeSnapshot();

        if (navigator != null)
        {
            navigator.SetAgentId(NavigationConsts.PlayerAgentId);
        }

        PlayerLocator.Instance.Register(entity);
        PlayerLocator.Instance.Register(playerInstance.transform);

        return true;
    }
}
