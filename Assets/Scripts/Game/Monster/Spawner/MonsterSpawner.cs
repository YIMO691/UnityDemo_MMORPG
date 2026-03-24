using UnityEngine;
using System.Collections.Generic;

public static class MonsterSpawner
{
    private static int spawnIndex = 0;
    public static GameObject SpawnMonster(int configId, Vector3 position)
    {
        return MonsterFactory.CreateNew(configId, position);
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
