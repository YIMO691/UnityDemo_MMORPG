using UnityEngine;

public class GameBootstrapper : MonoBehaviour
{
    private void Awake()
    {
        RuntimeLifecycleBootstrap.RegisterDefaults();
        RuntimeLifecycleRegistry.Instance.InitAll();
    }
}
