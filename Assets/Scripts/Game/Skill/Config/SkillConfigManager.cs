using System.Collections.Generic;
using UnityEngine;

public class SkillConfigManager
{
    private static readonly SkillConfigManager instance = new SkillConfigManager();
    public static SkillConfigManager Instance => instance;

    private readonly Dictionary<int, SkillConfig> dict = new Dictionary<int, SkillConfig>();
    private readonly List<SkillConfig> list = new List<SkillConfig>();
    private bool inited;

    public void Init()
    {
        if (inited) return;

        var ta = ResourceManager.Instance.Load<TextAsset>(AssetPaths.SkillConfig);
        if (ta != null)
        {
            var wrapper = JsonUtility.FromJson<SkillConfigList>(ta.text);
            dict.Clear();
            list.Clear();

            if (wrapper != null && wrapper.list != null)
            {
                for (int i = 0; i < wrapper.list.Count; i++)
                {
                    var cfg = wrapper.list[i];
                    if (cfg == null) continue;
                    list.Add(cfg);
                    dict[cfg.id] = cfg;
                }
            }
        }
        else
        {
            Debug.LogWarning("[SkillConfigManager] SkillConfig not found.");
        }

        inited = true;
    }

    public SkillConfig GetConfig(int skillId)
    {
        dict.TryGetValue(skillId, out var cfg);
        return cfg;
    }

    public List<SkillConfig> GetAllConfigs()
    {
        return list;
    }
}
