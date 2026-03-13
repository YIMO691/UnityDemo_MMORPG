using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ContinueSlotItem : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Text txtSlotName;
    [SerializeField] private Text txtRoleName;
    [SerializeField] private Text txtClassName;
    [SerializeField] private Text txtLevel;
    [SerializeField] private Text txtCreateTime;

    [SerializeField] private Button btnLoad;
    [SerializeField] private Button btnDel;

    private int slotId;
    private Action<int> onClickLoad;
    private Action<int> onClickDelete;

    public void Bind(PlayerSaveMetaData metaData, Action<int> onClickLoad, Action<int> onClickDelete)
    {
        if (metaData == null)
        {
            Debug.LogError("[ContinueSlotItem] metaData is null.");
            return;
        }

        this.slotId = metaData.slotId;
        this.onClickLoad = onClickLoad;
        this.onClickDelete = onClickDelete;

        if (txtSlotName != null)
            txtSlotName.text = metaData.slotId.ToString("D2");

        if (txtRoleName != null)
            txtRoleName.text = metaData.roleName;

        if (txtClassName != null)
            txtClassName.text = GetClassDisplayName(metaData.classId);

        if (txtLevel != null)
            txtLevel.text = "Lv " + metaData.level;

        if (txtCreateTime != null)
            txtCreateTime.text = metaData.saveTime;

        if (btnLoad != null)
        {
            btnLoad.onClick.RemoveAllListeners();
            btnLoad.onClick.AddListener(OnClickLoad);
        }

        if (btnDel != null)
        {
            btnDel.onClick.RemoveAllListeners();
            btnDel.onClick.AddListener(OnClickDelete);
        }
    }

    private void OnClickLoad()
    {
        onClickLoad?.Invoke(slotId);
    }

    private void OnClickDelete()
    {
        onClickDelete?.Invoke(slotId);
    }

    private string GetClassDisplayName(int classId)
    {
        RoleClassConfig config = RoleDataManager.Instance.GetClassConfig(classId);
        if (config == null)
        {
            return "未知职业";
        }

        return config.displayName;
    }
}
