using UnityEngine;

public static class MonsterAssembler
{
    public static bool TryAssemble(MonsterConfig config, Vector3 position, out GameObject monster, out MonsterEntity entity)
    {
        monster = null;
        entity = null;
        if (config == null || string.IsNullOrEmpty(config.prefabPath)) return false;
        var prefab = ResourceManager.Instance.Load<GameObject>(AssetPaths.MonsterRoot + config.prefabPath);
        if (prefab == null) return false;
        monster = Object.Instantiate(prefab, position, Quaternion.identity);
        var nav = monster.GetComponent<MonsterNavigator>();
        if (nav == null) nav = monster.AddComponent<MonsterNavigator>();
        var id = MonsterAgentId.Create(config.id, 0);
        nav.SetAgentId(id);
        NavigationRegistry.Instance.Register(nav);
        entity = monster.GetComponent<MonsterEntity>();
        if (entity == null) entity = monster.AddComponent<MonsterEntity>();
        entity.Init(config);
        return true;
    }
}
