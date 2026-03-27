using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryPanel : BasePanel
{
    public override UILayer Layer => UILayer.Popup;
    public override bool UseMask => true;
    public override bool CloseByMask => true;

    [Header("Header")]
    [SerializeField] private TMP_Text txtTitle;
    [SerializeField] private TMP_Text txtCapacity;
    [SerializeField] private Button btnClose;

    [Header("Scroll")]
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private RectTransform contentRoot;
    [SerializeField] private InventorySlotItem slotItemPrefab;

    [Header("Config")]
    [SerializeField] private int defaultSlotCount = 50;

    private readonly List<InventorySlotItem> slotItems = new List<InventorySlotItem>();
    private GridLayoutGroup gridLayoutGroup;
    private InventorySlotItem selectedSlotItem;

    protected override void OnCreate()
    {
        if (btnClose != null)
            btnClose.onClick.AddListener(OnClickClose);

        if (txtTitle != null)
            txtTitle.text = "背包";

        if (contentRoot != null)
            gridLayoutGroup = contentRoot.GetComponent<GridLayoutGroup>();

        EventBus.Subscribe<InventoryChangedEvent>(OnInventoryChanged);
    }

    protected override void OnShow()
    {
        RefreshInventoryView();

        if (scrollRect != null)
            scrollRect.verticalNormalizedPosition = 1f;
    }

    protected override void OnRefresh()
    {
        RefreshInventoryView();
    }

    protected override void OnDestroyPanel()
    {
        if (btnClose != null)
            btnClose.onClick.RemoveListener(OnClickClose);

        EventBus.Unsubscribe<InventoryChangedEvent>(OnInventoryChanged);
    }

    private void OnInventoryChanged(InventoryChangedEvent e)
    {
        Refresh();
    }

    private void OnClickClose()
    {
        UIManager.Instance.HidePanel<InventoryPanel>();
    }

    private void RefreshInventoryView()
    {
        ItemConfigManager.Instance.Init();

        PlayerData playerData = GamePlayerDataService.Instance.GetCurrentPlayerData();
        InventoryData inventoryData = playerData != null ? playerData.inventoryData : null;

        int slotCount = defaultSlotCount;
        int usedCount = 0;
        List<InventorySlotData> slotDataList = null;

        if (inventoryData != null)
        {
            if (inventoryData.slotCount > 0)
                slotCount = inventoryData.slotCount;

            if (inventoryData.slots != null)
            {
                slotDataList = inventoryData.slots;
                usedCount = inventoryData.slots.Count;
            }
        }

        EnsureSlotItemCount(slotCount);

        for (int i = 0; i < slotItems.Count; i++)
        {
            bool active = i < slotCount;
            slotItems[i].gameObject.SetActive(active);

            if (active)
            {
                slotItems[i].SetEmpty(i);
                slotItems[i].onClick = OnClickSlotItem;
            }
        }

        if (slotDataList != null)
        {
            for (int i = 0; i < slotDataList.Count; i++)
            {
                InventorySlotData slotData = slotDataList[i];
                if (slotData == null) continue;

                int slotIndex = slotData.slotIndex;
                if (slotIndex < 0 || slotIndex >= slotCount)
                {
                    Debug.LogWarning(
                        $"[InventoryPanel] slotIndex out of range. slotIndex={slotIndex}, slotCount={slotCount}, itemId={slotData.itemId}");
                    continue;
                }

                slotItems[slotIndex].Bind(slotData);
                slotItems[slotIndex].onClick = OnClickSlotItem;
            }
        }

        UpdateContentHeight(slotCount);

        if (txtCapacity != null)
        {
            txtCapacity.text = $"{usedCount}/{slotCount}";
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(contentRoot);

        if (scrollRect != null)
            scrollRect.verticalNormalizedPosition = 1f;

        ClearSelection();
    }

    private void EnsureSlotItemCount(int count)
    {
        if (contentRoot == null)
        {
            Debug.LogError("[InventoryPanel] contentRoot is null.");
            return;
        }

        if (slotItemPrefab == null)
        {
            Debug.LogError("[InventoryPanel] slotItemPrefab is null.");
            return;
        }

        while (slotItems.Count < count)
        {
            InventorySlotItem item = Instantiate(slotItemPrefab, contentRoot);
            item.onClick = OnClickSlotItem;
            slotItems.Add(item);
        }
    }

    private void UpdateContentHeight(int slotCount)
    {
        if (contentRoot == null || gridLayoutGroup == null)
            return;

        int columnCount = Mathf.Max(1, gridLayoutGroup.constraintCount);
        int rowCount = Mathf.CeilToInt(slotCount / (float)columnCount);

        float cellHeight = gridLayoutGroup.cellSize.y;
        float spacingY = gridLayoutGroup.spacing.y;
        float paddingTop = gridLayoutGroup.padding.top;
        float paddingBottom = gridLayoutGroup.padding.bottom;

        float height = paddingTop + paddingBottom;
        if (rowCount > 0)
        {
            height += rowCount * cellHeight;
            height += (rowCount - 1) * spacingY;
        }

        Vector2 sizeDelta = contentRoot.sizeDelta;
        sizeDelta.y = height;
        contentRoot.sizeDelta = sizeDelta;
    }

    private void OnClickSlotItem(InventorySlotItem slotItem)
    {
        SelectSlot(slotItem);
    }

    private void SelectSlot(InventorySlotItem slotItem)
    {
        if (slotItem == null)
        {
            ClearSelection();
            return;
        }

        if (selectedSlotItem != null)
        {
            selectedSlotItem.SetSelected(false);
        }

        selectedSlotItem = slotItem;
        selectedSlotItem.SetSelected(true);
    }

    private void ClearSelection()
    {
        if (selectedSlotItem != null)
        {
            selectedSlotItem.SetSelected(false);
            selectedSlotItem = null;
        }
    }
}
