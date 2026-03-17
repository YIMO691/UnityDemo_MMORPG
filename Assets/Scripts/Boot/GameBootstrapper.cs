using UnityEngine;

public class GameBootstrapper : MonoBehaviour
{
    private void Awake()
    {
        if (!DataManager.Instance.IsInited)
        {
            Debug.LogWarning("[GameBootstrapper] DataManager not initialized, initializing as fallback.");
            DataManager.Instance.Init();
        }
        if (!UIManager.Instance.IsInited)
        {
            Debug.LogWarning("[GameBootstrapper] UIManager not initialized, initializing as fallback.");
            UIManager.Instance.Init();
        }
        CreateRoleFlowController.Instance.Init();
    }
}
