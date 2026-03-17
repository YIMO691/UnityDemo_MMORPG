using UnityEngine;

public class GameBootstrapper : MonoBehaviour
{
    private void Awake()
    {
        DataManager.Instance.Init();
        UIManager.Instance.Init();
        CreateRoleFlowController.Instance.Init();
    }
}

