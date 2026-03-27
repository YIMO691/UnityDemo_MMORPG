using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemDetailPopup : BasePanel
{
    public override UILayer Layer => UILayer.Popup;
    public override bool UseMask => true;
    public override bool CloseByMask => true;

    [SerializeField] private Text txtTitle;
    [SerializeField] private TMP_Text txtItemId;
    [SerializeField] private Text txtCount;
    [SerializeField] private Text txtDesc;

    [SerializeField] private Button btnUse;
    [SerializeField] private Button btnDrop;
    [SerializeField] private Button btnClose;

    private ItemDetailContext context;

    public void SetData(ItemDetailContext context)
    {
        this.context = context;
    }

    protected override void OnCreate()
    {
        if (btnClose != null)
            btnClose.onClick.AddListener(OnClickClose);

        if (btnUse != null)
            btnUse.onClick.AddListener(OnClickUse);

        if (btnDrop != null)
            btnDrop.onClick.AddListener(OnClickDrop);
    }

    protected override void OnShow()
    {
        RefreshView();
    }

    protected override void OnRefresh()
    {
        RefreshView();
    }

    protected override void OnDestroyPanel()
    {
        if (btnClose != null)
            btnClose.onClick.RemoveListener(OnClickClose);

        if (btnUse != null)
            btnUse.onClick.RemoveListener(OnClickUse);

        if (btnDrop != null)
            btnDrop.onClick.RemoveListener(OnClickDrop);
    }

    private void OnClickClose()
    {
        UIManager.Instance.HidePanel<ItemDetailPopup>();
    }

    private void OnClickUse()
    {
        if (context == null) return;
        bool success = InventoryActionService.TryUseItem(context);
        if (!success) return;
        UIManager.Instance.HidePanel<ItemDetailPopup>();
    }

    private void OnClickDrop()
    {
        if (context == null) return;

        ItemConfig cfg = ItemConfigManager.Instance.GetConfig(context.itemId);
        string itemName = cfg != null ? cfg.name : $"Item {context.itemId}";
        string message = $"是否确认丢弃 {itemName} x{context.count}？";

        ConfirmPanel panel = UIManager.Instance.ShowPanel<ConfirmPanel>();
        if (panel != null)
        {
            panel.SetData(
                message,
                onConfirm: () =>
                {
                    bool success = InventoryActionService.TryDropItem(context);
                    if (!success) return;

                    UIManager.Instance.HidePanel<ItemDetailPopup>();
                },
                onCancel: () =>
                {
                }
            );

            panel.Refresh();
        }

    }

    private void RefreshView()
    {
        if (context == null)
        {
            SetEmptyView();
            return;
        }

        if (context.isEmpty)
        {
            SetEmptySlotView();
            return;
        }

        ItemConfig cfg = ItemConfigManager.Instance.GetConfig(context.itemId);
        string itemName = cfg != null ? cfg.name : $"Item {context.itemId}";

        if (txtTitle != null) txtTitle.text = itemName;
        if (txtItemId != null) txtItemId.text = $"ItemId: {context.itemId}";
        if (txtCount != null) txtCount.text = $"数量: {context.count}";
        if (txtDesc != null) txtDesc.text = cfg != null ? cfg.desc : "描述缺失。";

        if (btnUse != null)
            btnUse.gameObject.SetActive(cfg != null && cfg.canUse);

        if (btnDrop != null)
            btnDrop.gameObject.SetActive(cfg == null || cfg.canDrop);
    }

    private void SetEmptyView()
    {
        if (txtTitle != null) txtTitle.text = "未选择物品";
        if (txtItemId != null) txtItemId.text = string.Empty;
        if (txtCount != null) txtCount.text = string.Empty;
        if (txtDesc != null) txtDesc.text = string.Empty;

        if (btnUse != null) btnUse.gameObject.SetActive(false);
        if (btnDrop != null) btnDrop.gameObject.SetActive(false);
    }

    private void SetEmptySlotView()
    {
        if (txtTitle != null) txtTitle.text = "空槽位";
        if (txtItemId != null) txtItemId.text = string.Empty;
        if (txtCount != null) txtCount.text = string.Empty;
        if (txtDesc != null) txtDesc.text = "当前槽位没有物品。";

        if (btnUse != null) btnUse.gameObject.SetActive(false);
        if (btnDrop != null) btnDrop.gameObject.SetActive(false);
    }
}
