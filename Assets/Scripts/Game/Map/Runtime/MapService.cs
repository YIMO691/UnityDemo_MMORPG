using UnityEngine;

public class MapService
{
    private static readonly MapService instance = new MapService();
    public static MapService Instance => instance;

    private MapService() { }

    public string GetCurrentScene()
    {
        var data = GamePlayerDataService.Instance.GetCurrentPlayerData();
        return data?.runtimeData?.Scene;
    }

    public Vector3 GetPlayerPosition()
    {
        var data = GamePlayerDataService.Instance.GetCurrentPlayerData();
        if (data?.runtimeData == null) return Vector3.zero;

        return new Vector3(
            data.runtimeData.posX,
            data.runtimeData.posY,
            data.runtimeData.posZ
        );
    }

    public MapConfig GetCurrentMapConfig()
    {
        string scene = GetCurrentScene();
        return MapDataManager.Instance.GetByScene(scene);
    }
}
