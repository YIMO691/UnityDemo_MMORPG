using System.Collections.Generic;
using UnityEngine;

public class MapDataManager
{
    private static readonly MapDataManager instance = new MapDataManager();
    public static MapDataManager Instance => instance;

    private readonly Dictionary<string, MapConfig> mapDict = new Dictionary<string, MapConfig>();

    private MapDataManager() { }

    public void Init()
    {
        LoadMapConfigs();
    }

    private void LoadMapConfigs()
    {
        TextAsset json = ResourceManager.Instance.Load<TextAsset>(AssetPaths.MapConfig);
        if (json == null)
        {
            Debug.LogError($"[MapDataManager] MapConfig not found: {AssetPaths.MapConfig}");
            return;
        }

        MapConfigList wrapper = JsonUtility.FromJson<MapConfigList>(json.text);
        mapDict.Clear();

        if (wrapper != null && wrapper.list != null)
        {
            foreach (var item in wrapper.list)
            {
                if (string.IsNullOrEmpty(item.sceneName)) continue;
                mapDict[item.sceneName] = item;
            }
            Debug.Log($"[MapDataManager] LoadMapConfigs Success. Count = {mapDict.Count}");
        }
        else
        {
            Debug.LogError("[MapDataManager] Parse MapConfig failed.");
        }
    }

    public MapConfig GetByScene(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName)) return null;
        mapDict.TryGetValue(sceneName, out var config);
        return config;
    }
}
