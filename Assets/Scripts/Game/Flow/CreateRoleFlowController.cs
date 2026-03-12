using UnityEngine;

public class CreateRoleFlowController
{
    private static readonly CreateRoleFlowController instance = new CreateRoleFlowController();
    public static CreateRoleFlowController Instance => instance;

    private bool isInited = false;

    private CreateRoleFlowController() { }

    public void Init()
    {
        if (isInited) return;

        EventBus.Subscribe<CreateRoleRequestEvent>(OnCreateRoleRequestEvent);

        isInited = true;
    }

    public void Clear()
    {
        if (!isInited) return;

        EventBus.Unsubscribe<CreateRoleRequestEvent>(OnCreateRoleRequestEvent);

        isInited = false;
    }

    private void OnCreateRoleRequestEvent(CreateRoleRequestEvent e)
    {
        PlayerData playerData = PlayerFactory.CreatePlayerData(e.Request);
        if (playerData == null)
        {
            Debug.LogError("[CreateRoleFlowController] 创建角色失败。");
            return;
        }

        DataManager.Instance.SetCurrentPlayerData(playerData);

        Debug.Log($"[CreateRoleFlowController] 创角成功：{playerData.baseData.roleName}");

        // 这里后续再接主界面或加载场景
        // UIManager.Instance.ShowMainPage<MainPanel>(hideOld: true, useFade: false);
    }
}
