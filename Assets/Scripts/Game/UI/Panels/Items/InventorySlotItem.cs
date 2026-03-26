using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventorySlotItem : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject emptyRoot;
    [SerializeField] private GameObject contentRoot;

    [SerializeField] private Image imgIcon;
    [SerializeField] private TMP_Text txtItemName;
    [SerializeField] private TMP_Text txtCount;

    [Header("Optional")]
    [SerializeField] private Sprite defaultIcon;

    private int slotIndex = -1;

    public void SetEmpty(int slotIndex)
    {
        this.slotIndex = slotIndex;

        if (emptyRoot != null) emptyRoot.SetActive(true);
        if (contentRoot != null) contentRoot.SetActive(false);

        if (txtItemName != null) txtItemName.text = string.Empty;
        if (txtCount != null) txtCount.text = string.Empty;
        if (imgIcon != null) imgIcon.sprite = defaultIcon;
    }

    public void Bind(InventorySlotData slotData)
    {
        if (slotData == null)
        {
            SetEmpty(-1);
            return;
        }

        slotIndex = slotData.slotIndex;

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
            // V1 先不走 iconPath，统一占位图
            imgIcon.sprite = defaultIcon;
        }
    }
}
