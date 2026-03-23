using UnityEngine;
using System.Collections.Generic;

public static class MonsterSpawner
{
    private static int spawnIndex = 0;
    public static GameObject SpawnMonster(int configId, Vector3 position)
    {
        var cfg = MonsterConfigManager.Instance.GetConfig(configId);
        if (cfg == null) return null;
        spawnIndex++;
        if (MonsterAssembler.TryAssemble(cfg, position, spawnIndex, out var go, out var _))
        {
            return go;
        }
        return null;
    }

    public static GameObject SpawnFirst(Vector3 position)
    {
        List<MonsterConfig> all = MonsterConfigManager.Instance.GetAllConfigs();
        if (all != null && all.Count > 0)
        {
            return SpawnMonster(all[0].id, position);
        }
        return null;
    }
}
