using System.Collections.Generic;
using UnityEngine;

public class MonsterConfigManager
{
    private static readonly MonsterConfigManager instance = new MonsterConfigManager();
    public static MonsterConfigManager Instance => instance;

    private readonly Dictionary<int, MonsterConfig> dict = new Dictionary<int, MonsterConfig>();
    private readonly List<MonsterConfig> list = new List<MonsterConfig>();
    private bool inited;

    public void Init()
    {
        if (inited) return;
        var ta = ResourceManager.Instance.Load<TextAsset>("Config/MonsterConfig");
        if (ta != null)
        {
            var wrapper = JsonUtility.FromJson<MonsterConfigList>(ta.text);
            list.Clear();
            dict.Clear();
            if (wrapper != null && wrapper.list != null)
            {
                for (int i = 0; i < wrapper.list.Count; i++)
                {
                    var cfg = wrapper.list[i];
                    list.Add(cfg);
                    dict[cfg.id] = cfg;
                }
            }
        }
        inited = true;
    }

    public MonsterConfig GetConfig(int id)
    {
        dict.TryGetValue(id, out var cfg);
        return cfg;
    }

    public List<MonsterConfig> GetAllConfigs()
    {
        return list;
    }
}
