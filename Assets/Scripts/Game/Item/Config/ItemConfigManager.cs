using System.Collections.Generic;
using UnityEngine;

public class ItemConfigManager
{
    private static readonly ItemConfigManager instance = new ItemConfigManager();
    public static ItemConfigManager Instance => instance;

    private readonly Dictionary<int, ItemConfig> dict = new Dictionary<int, ItemConfig>();
    private bool inited;

    public void Init()
    {
        if (inited) return;
        var ta = ResourceManager.Instance.Load<TextAsset>(AssetPaths.ItemConfig);
        if (ta != null)
        {
            var wrapper = JsonUtility.FromJson<ItemConfigList>(ta.text);
            dict.Clear();
            if (wrapper != null && wrapper.list != null)
            {
                for (int i = 0; i < wrapper.list.Count; i++)
                {
                    var cfg = wrapper.list[i];
                    dict[cfg.id] = cfg;
                }
            }
        }
        inited = true;
    }

    public ItemConfig GetConfig(int id)
    {
        dict.TryGetValue(id, out var cfg);
        return cfg;
    }
}
