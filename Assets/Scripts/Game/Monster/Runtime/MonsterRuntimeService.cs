using UnityEngine;

public static class MonsterRuntimeService
{
    private static int idCounter = 0;

    public static GameObject CreateFromSpawnPoint(int configId, Vector3 position, string spawnPointId)
    {
        var config = MonsterFactory.GetConfig(configId);
        if (config == null) return null;

        int spawnIndex = ++idCounter;

        if (!MonsterAssembler.TryAssemble(config, position, spawnIndex, out var go, out var entity))
            return null;

        string runtimeId = BuildRuntimeId(config.id, spawnIndex);
        entity.SetIdentity(runtimeId, spawnPointId);

        MonsterRuntimeRegistry.Instance.Register(entity);
        return go;
    }

    public static GameObject RestoreFromSave(MonsterSaveData data)
    {
        if (data == null) return null;
        if (data.isDead) return null;
        if (MonsterRuntimeRegistry.Instance.HasAlive(data.runtimeId)) return null;

        var config = MonsterFactory.GetConfig(data.configId);
        if (config == null) return null;

        SyncCounterFromRuntimeId(data.runtimeId);

        Vector3 position = new Vector3(data.posX, data.posY, data.posZ);
        Vector3 home = ResolveHomePosition(data.spawnPointId, position);

        if (!MonsterAssembler.TryAssemble(config, position, 0, out var go, out var entity))
            return null;

        entity.SetIdentity(data.runtimeId, data.spawnPointId);
        entity.ApplySaveData(data);
        entity.SetSpawnPosition(home);

        if (entity.IsDead) return null;

        MonsterRuntimeRegistry.Instance.Register(entity);
        return go;
    }

    private static string BuildRuntimeId(int configId, int spawnIndex)
    {
        return $"{configId}_{spawnIndex}";
    }

    private static void SyncCounterFromRuntimeId(string runtimeId)
    {
        if (string.IsNullOrEmpty(runtimeId)) return;
        var parts = runtimeId.Split('_');
        if (parts.Length < 2) return;
        if (int.TryParse(parts[1], out int savedIndex))
        {
            if (savedIndex > idCounter) idCounter = savedIndex;
        }
    }

    private static Vector3 ResolveHomePosition(string spawnPointId, Vector3 fallback)
    {
        if (string.IsNullOrEmpty(spawnPointId)) return fallback;
        var points = Object.FindObjectsOfType<MonsterSpawnPoint>();
        for (int i = 0; i < points.Length; i++)
        {
            if (points[i] != null && points[i].spawnPointId == spawnPointId)
                return points[i].transform.position;
        }
        return fallback;
    }
}
