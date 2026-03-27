using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventorySlotItem : MonoBehaviour, IPointerClickHandler
{
    [Header("UI")]
    [SerializeField] private GameObject emptyRoot;
    [SerializeField] private GameObject contentRoot;
    [SerializeField] private GameObject selectionRoot;

    [SerializeField] private Image imgIcon;
    [SerializeField] private TMP_Text txtItemName;
    [SerializeField] private TMP_Text txtCount;

    [Header("Optional")]
    [SerializeField] private Sprite defaultIcon;

    private int slotIndex = -1;
    private InventorySlotData slotData;
    private bool isEmpty = true;

    public Action<InventorySlotItem> onClick;

    public int SlotIndex => slotIndex;
    public InventorySlotData SlotData => slotData;
    public bool IsEmpty => isEmpty;

    public void SetEmpty(int slotIndex)
    {
        this.slotIndex = slotIndex;
        this.slotData = null;
        this.isEmpty = true;

        if (emptyRoot != null) emptyRoot.SetActive(true);
        if (contentRoot != null) contentRoot.SetActive(false);

        if (txtItemName != null) txtItemName.text = string.Empty;
        if (txtCount != null) txtCount.text = string.Empty;
        if (imgIcon != null) imgIcon.sprite = defaultIcon;

        SetSelected(false);
    }

    public void Bind(InventorySlotData slotData)
    {
        if (slotData == null)
        {
            SetEmpty(-1);
            return;
        }

        this.slotIndex = slotData.slotIndex;
        this.slotData = slotData;
        this.isEmpty = false;

        if (emptyRoot != null) emptyRoot.SetActive(false);
        if (contentRoot != null) contentRoot.SetActive(true);

        ItemConfig cfg = ItemConfigManager.Instance.GetConfig(slotData.itemId);

        if (txtItemName != null)
        {
            txtItemName.text = cfg != null ? cfg.name : $"Item {slotData.itemId}";
        }

        if (txtCount != null)
        {
            txtCount.text = slotData.count > 1 ? slotData.count.ToString() : string.Empty;
        }

        if (imgIcon != null)
        {
            imgIcon.sprite = defaultIcon;
        }

        SetSelected(false);
    }

    public void SetSelected(bool selected)
    {
        if (selectionRoot != null)
        {
            selectionRoot.SetActive(selected);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        onClick?.Invoke(this);

        ItemDetailContext context = new ItemDetailContext
        {
            itemId = slotData != null ? slotData.itemId : 0,
            count = slotData != null ? slotData.count : 0,
            slotIndex = slotIndex,
            isEmpty = isEmpty,
            source = "Inventory"
        };

        EventBus.Publish(new OpenItemDetailPopupEvent(context));
    }
}
