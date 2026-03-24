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

    public List<MonsterEntity> GetAll()
    {
        return new List<MonsterEntity>(map.Values);
    }

    public void Clear()
    {
        map.Clear();
    }
}
