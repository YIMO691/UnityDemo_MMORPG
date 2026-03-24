using UnityEngine;

public static class MonsterModule
{
    public static void InitForScene()
    {
        RestoreMonstersIfAny();
        InitSpawnPoints();
    }

    public static void RestoreMonstersIfAny()
    {
        var data = GamePlayerDataService.Instance.GetCurrentPlayerData();
        if (data == null) return;
        if (data.monsterData == null || data.monsterData.Count == 0) return;

        var svc = new MonsterSaveService();
        svc.RestoreScene(data.monsterData);
    }

    public static void InitSpawnPoints()
    {
        var spawnPoints = Object.FindObjectsOfType<MonsterSpawnPoint>();
        Debug.Log($"[Monster] SpawnPoints found: {spawnPoints.Length}");
        for (int i = 0; i < spawnPoints.Length; i++)
        {
            if (spawnPoints[i] != null)
                spawnPoints[i].Init();
        }
    }

    public static int CountAliveBySpawnPoint(string spawnPointId)
    {
        return MonsterRuntimeRegistry.Instance.CountAliveBySpawnPoint(spawnPointId);
    }

    public static bool HasAliveByRuntimeId(string runtimeId)
    {
        return MonsterRuntimeRegistry.Instance.HasAlive(runtimeId);
    }
}
