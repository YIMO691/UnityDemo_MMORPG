using UnityEngine;

public static class AppRuntimeInitializer
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Init()
    {
        DataManager.Instance.Init();
        UIManager.Instance.Init();
        RoleDataManager.Instance.Init();
        MapDataManager.Instance.Init();
        CreateRoleFlowController.Instance.Init();
        RoleUIController.Instance.Init();
        NavigationService.Instance.Init();
    }
}
