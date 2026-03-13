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

        int slotId = DataManager.Instance.GetNextAvailableSlotId();
        DataManager.Instance.SaveCurrentPlayerDataToSlot(slotId);

        Debug.Log("[CreateRoleFlowController] 创角成功，保存到槽位：" + slotId);

        EventBus.Publish(new OpenMainPageEvent("MainPanel", true, false));
    }



}
