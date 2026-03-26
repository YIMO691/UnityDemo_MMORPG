using System.Collections.Generic;
using UnityEngine;

public class DropTableConfigManager
{
    private static readonly DropTableConfigManager instance = new DropTableConfigManager();
    public static DropTableConfigManager Instance => instance;

    private readonly Dictionary<int, DropTableConfig> dict = new Dictionary<int, DropTableConfig>();
    private readonly List<DropTableConfig> list = new List<DropTableConfig>();
    private bool inited;

    public void Init()
    {
        if (inited) return;
        var ta = ResourceManager.Instance.Load<TextAsset>(AssetPaths.DropTableConfig);
        if (ta != null)
        {
            var wrapper = JsonUtility.FromJson<DropTableConfigList>(ta.text);
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

    public DropTableConfig GetConfig(int id)
    {
        dict.TryGetValue(id, out var cfg);
        return cfg;
    }
}
