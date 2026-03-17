using Game.Runtime;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ContinuePanel : BasePanel
{
    public override UILayer Layer => UILayer.Popup;
    public override bool UseMask => true;
    public override bool CloseByMask => true;

    [Header("UI")]
    [SerializeField] private Transform contentRoot;
    [SerializeField] private ContinueSlotItem slotItemPrefab;
    [SerializeField] private Button btnClose;

    [Header("Config")]
    [SerializeField] private int maxSlotCount = 20;

    private readonly List<ContinueSlotItem> slotItemList = new List<ContinueSlotItem>();

    protected override void OnCreate()
    {
        if (btnClose != null)
        {
            btnClose.onClick.AddListener(OnClickClose);
        }
    }

    protected override void OnShow()
    {
        base.OnShow();
        RefreshList();
    }

    protected override void OnDestroyPanel()
    {
        if (btnClose != null)
        {
            btnClose.onClick.RemoveListener(OnClickClose);
        }

        base.OnDestroyPanel();
    }

    private void RefreshList()
    {
        ClearList();

        List<PlayerSaveMetaData> metaList = new List<PlayerSaveMetaData>();
        for (int i = 1; i <= maxSlotCount; i++)
        {
            if (DataManager.Instance.HasPlayerSaveInSlot(i))
            {
                var data = DataManager.Instance.GetPlayerDataFromSlot(i);
                var meta = PlayerSaveMetaMapper.Map(data, i);
                if (meta != null) metaList.Add(meta);
            }
        }

        for (int i = 0; i < metaList.Count; i++)
        {
            ContinueSlotItem item = GameObject.Instantiate(slotItemPrefab, contentRoot);
            item.Bind(metaList[i], OnClickLoadSlot, OnClickDeleteSlot);
            slotItemList.Add(item);
        }

        // 如果没有存档，也可以弹提示；或者保留空白
        if (metaList.Count == 0)
        {
            ShowMessage("当前没有存档");
        }
    }

    private void ClearList()
    {
        for (int i = 0; i < slotItemList.Count; i++)
        {
            if (slotItemList[i] != null)
            {
                Destroy(slotItemList[i].gameObject);
            }
        }

        slotItemList.Clear();
    }

    private void OnClickLoadSlot(int slotId)
    {
        bool success = DataManager.Instance.LoadPlayerDataFromSlot(slotId);
        if (!success)
        {
            ShowMessage("读取存档失败");
            return;
        }

        PlayerData playerData = DataManager.Instance.GetCurrentPlayerData();
        if (playerData == null)
        {
            ShowMessage("当前角色数据为空");
            return;
        }

        GameRuntime.CurrentPlayerData = playerData;
        GameRuntime.CurrentSlotId = DataManager.Instance.GetCurrentSlotId();

        EventBus.Publish(new ClosePanelEvent(UIRouteNames.ContinuePanel));
        SceneNavigator.EnterGameScene();
    }


    private void OnClickDeleteSlot(int slotId)
    {
        UIManager.Instance.ShowConfirm(
            $"是否删除存档 {slotId:D2}？",
            () =>
            {
                DataManager.Instance.DeletePlayerDataInSlot(slotId);
                RefreshList();
                ShowMessage("存档已删除");
            },
            null
        );
    }


    private void OnClickClose()
    {
        EventBus.Publish(new ClosePanelEvent(UIRouteNames.ContinuePanel));
    }

    private void ShowMessage(string message)
    {
        UIManager.Instance.ShowPanel<MessageTipPanel>();
        MessageTipPanel panel = UIManager.Instance.GetPanel<MessageTipPanel>();
        if (panel != null)
        {
            panel.SetMessage(message);
        }
    }

}
