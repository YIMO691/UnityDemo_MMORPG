using Game.Runtime;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CreateRoleFlowController
{
    private static readonly CreateRoleFlowController instance = new CreateRoleFlowController();
    public static CreateRoleFlowController Instance => instance;

    private bool isInited = false;
    public bool IsInited => isInited;

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
        if (e.Request == null)
        {
            Debug.LogError("[CreateRoleFlowController] CreateRoleRequestEvent 或 Request 为空。");
            return;
        }

        PlayerData playerData = PlayerFactory.CreatePlayerData(e.Request);
        if (playerData == null)
        {
            Debug.LogError("[CreateRoleFlowController] 创建角色失败。");
            return;
        }

        int slotId = DataManager.Instance.GetNextAvailableSlotId();
        if (slotId < 1)
        {
            Debug.LogError("[CreateRoleFlowController] 没有可用存档槽位。");
            return;
        }

        GamePlayerDataService.Instance.SavePlayerDataToSlot(slotId, playerData);

        Debug.Log("[CreateRoleFlowController] 创角成功，保存到槽位：" + slotId);

        // 立刻注入内存态，确保主界面与头像详情可读取当前角色信息
        GamePlayerDataService.Instance.SetCurrentPlayerData(playerData);
        // SavePlayerDataToSlot 已设置当前槽位，无需重复设置
        
        SceneNavigator.EnterGameScene();
    }
}
