using System.Collections.Generic;
using UnityEngine;

public class RoleDataManager
{
    private static readonly RoleDataManager instance = new RoleDataManager();
    public static RoleDataManager Instance => instance;

    private List<RoleClassConfig> classConfigs = new List<RoleClassConfig>();

    private RoleDataManager() { }

    public void Init()
    {
        LoadRoleClassConfigs();
    }

    private void LoadRoleClassConfigs()
    {
        TextAsset json = ResourceManager.Instance.Load<TextAsset>(AssetPaths.RoleClassConfig);
        if (json == null)
        {
            Debug.LogError($"[RoleDataManager] RoleClassConfig not found: {AssetPaths.RoleClassConfig}");
            return;
        }

        RoleClassConfigList wrapper = JsonUtility.FromJson<RoleClassConfigList>(json.text);
        if (wrapper != null && wrapper.list != null)
        {
            classConfigs = wrapper.list;
            Debug.Log($"[RoleDataManager] LoadRoleClassConfigs Success. Count = {classConfigs.Count}");
        }
        else
        {
            Debug.LogError("[RoleDataManager] Parse RoleClassConfig failed.");
        }
    }

    public List<RoleClassConfig> GetAllClassConfigs()
    {
        return classConfigs;
    }

    public RoleClassConfig GetClassConfig(int classId)
    {
        return classConfigs.Find(c => c.id == classId);
    }
}
