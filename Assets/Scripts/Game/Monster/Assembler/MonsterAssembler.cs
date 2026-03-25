using UnityEngine;

public static class MonsterAssembler
{
    public static bool TryAssemble(MonsterConfig config, Vector3 position, int spawnIndex, out GameObject monster, out MonsterEntity entity)
    {
        monster = null;
        entity = null;
        if (config == null || string.IsNullOrEmpty(config.prefabPath)) return false;
        var prefab = ResourceManager.Instance.Load<GameObject>(AssetPaths.MonsterRoot + config.prefabPath);
        if (prefab == null) return false;
        monster = Object.Instantiate(prefab, position, Quaternion.identity);
        var nav = monster.GetComponent<MonsterNavigator>() ?? monster.AddComponent<MonsterNavigator>();

        entity = monster.GetComponent<MonsterEntity>();
        if (entity == null) entity = monster.AddComponent<MonsterEntity>();

        var anim = monster.GetComponent<MonsterAnimatorDriver>() ?? monster.AddComponent<MonsterAnimatorDriver>();
        var exec = monster.GetComponent<MonsterLocomotionExecutor>() ?? monster.AddComponent<MonsterLocomotionExecutor>();
        var brain = monster.GetComponent<MonsterBrain>() ?? monster.AddComponent<MonsterBrain>();

        entity.Init(config, position);
        var id = MonsterAgentId.Create(config.id, spawnIndex);
        nav.SetAgentId(id);
        NavigationRegistry.Instance.Register(nav);
        if (monster.GetComponent<MonsterAnimationEvents>() == null) monster.AddComponent<MonsterAnimationEvents>();
        return true;
    }
}
