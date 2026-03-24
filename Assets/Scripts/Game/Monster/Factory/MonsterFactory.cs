using UnityEngine;

public static class MonsterFactory
{
    private static int _idCounter = 0;

    public static GameObject CreateNew(int configId, Vector3 pos, string spawnPointId = null)
    {
        var cfg = MonsterConfigManager.Instance.GetConfig(configId);
        if (cfg == null) return null;
        int spawnIndex = ++_idCounter;
        if (!MonsterAssembler.TryAssemble(cfg, pos, spawnIndex, out var go, out var entity)) return null;
        string runtimeId = $"{configId}_{spawnIndex}";
        entity.SetIdentity(runtimeId, spawnPointId);
        MonsterRuntimeRegistry.Instance.Register(entity);
        return go;
    }

    public static GameObject RestoreFromSave(MonsterSaveData data)
    {
        var cfg = MonsterConfigManager.Instance.GetConfig(data.configId);
        if (cfg == null) return null;
        Vector3 pos = new Vector3(data.posX, data.posY, data.posZ);
        if (!MonsterAssembler.TryAssemble(cfg, pos, 0, out var go, out var entity)) return null;
        entity.SetIdentity(data.runtimeId, data.spawnPointId);
        entity.ApplySaveData(data);
        MonsterRuntimeRegistry.Instance.Register(entity);
        return go;
    }
}
