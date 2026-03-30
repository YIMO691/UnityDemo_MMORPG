using UnityEngine;

public static class AppRuntimeInitializer
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Init()
    {
        RuntimeLifecycleBootstrap.RegisterDefaults();
        RuntimeLifecycleRegistry.Instance.InitAll();
    }
}
