using UnityEngine;
using UnityEngine.UI;

public class RoleInfoPanel : BasePanel
{
    public override UILayer Layer => UILayer.Popup;
    public override bool UseMask => true;
    public override bool CloseByMask => true;

    [Header("Texts")]
    public Text txtTitle;
    public Text txtRoleName;
    public Text txtRoleType;
    public Text txtDesc;
    public Text txtBaseAttribute;

    [Header("Buttons")]
    public Button btnClose;

    private RoleClassConfig currentConfig;

    protected override void OnCreate()
    {
        if (btnClose != null)
        {
            btnClose.onClick.AddListener(OnClickClose);
        }
    }

    protected override void OnShow()
    {
        RefreshInfo();
    }

    protected override void OnDestroyPanel()
    {
        if (btnClose != null)
        {
            btnClose.onClick.RemoveListener(OnClickClose);
        }
    }

    public void SetRoleInfo(RoleClassConfig config)
    {
        currentConfig = config;

        if (IsVisible)
        {
            RefreshInfo();
        }
    }

    private void RefreshInfo()
    {
        if (currentConfig == null)
        {
            txtTitle.text = "职业详情";
            txtRoleName.text = "";
            txtRoleType.text = "";
            txtDesc.text = "";
            txtBaseAttribute.text = "";

            return;
        }

        txtTitle.text = "职业详情";
        txtRoleName.text = currentConfig.displayName;
        txtRoleType.text = $"定位：{GetRoleTypeDisplayName(currentConfig.roleType)}";
        txtDesc.text = currentConfig.description;

        txtBaseAttribute.text =
            $"初始等级：{currentConfig.baseLevel}\n" +
            $"当前经验：{currentConfig.baseExp}\n" +
            $"升级需求：{currentConfig.baseExpToLevel}\n" +
            $"经验倍率：{currentConfig.expGrowthRate:F2}\n\n" +
            $"生命：{currentConfig.maxHp}\n" +
            $"能量：{currentConfig.maxMp}\n" +
            $"攻击：{currentConfig.attack}\n" +
            $"防御：{currentConfig.defense}\n" +
            $"速度：{currentConfig.speed}";
    }

    private void OnClickClose()
    {
        UIManager.Instance.HidePanel<RoleInfoPanel>();
    }

    private string GetRoleTypeDisplayName(string roleType)
    {
        switch (roleType)
        {
            case "frontline":
                return "前排";
            case "ranged_dps":
                return "远程输出";
            case "support":
                return "辅助";
            case "utility":
                return "功能型";
            default:
                return "未知";
        }
    }
}
