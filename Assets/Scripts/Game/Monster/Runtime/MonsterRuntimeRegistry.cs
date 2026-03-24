using System.Collections.Generic;

public class MonsterRuntimeRegistry
{
    private static readonly MonsterRuntimeRegistry instance = new MonsterRuntimeRegistry();
    public static MonsterRuntimeRegistry Instance => instance;

    private readonly Dictionary<string, MonsterEntity> map = new Dictionary<string, MonsterEntity>();

    public void Register(MonsterEntity entity)
    {
        if (entity == null) return;
        if (string.IsNullOrEmpty(entity.RuntimeId)) return;

        map[entity.RuntimeId] = entity;
    }

    public void Unregister(MonsterEntity entity)
    {
        if (entity == null) return;
        if (string.IsNullOrEmpty(entity.RuntimeId)) return;

        map.Remove(entity.RuntimeId);
    }

    public MonsterEntity Get(string runtimeId)
    {
        if (string.IsNullOrEmpty(runtimeId)) return null;

        map.TryGetValue(runtimeId, out var entity);
        return entity;
    }

    public bool HasAlive(string runtimeId)
    {
        var entity = Get(runtimeId);
        return entity != null && !entity.IsDead;
    }

    public int CountAliveBySpawnPoint(string spawnPointId)
    {
        if (string.IsNullOrEmpty(spawnPointId)) return 0;

        int count = 0;
        foreach (var kv in map)
        {
            var entity = kv.Value;
            if (entity == null) continue;
            if (entity.IsDead) continue;
            if (entity.SpawnPointId != spawnPointId) continue;
            count++;
        }
        return count;
    }

    public List<MonsterEntity> GetAll()
    {
        return new List<MonsterEntity>(map.Values);
    }

    public void Clear()
    {
        map.Clear();
    }
}
