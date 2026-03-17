using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AboutPanel : BasePanel
{
    public override UILayer Layer => UILayer.Popup;
    public override bool UseMask => true;
    public override bool CloseByMask => true;

    [Header("UI References")]
    [SerializeField] private Text txtTitle;
    [SerializeField] private Text txtContent;
    [SerializeField] private Button btnClose;

    protected override void OnCreate()
    {
        if (btnClose != null)
        {
            btnClose.onClick.AddListener(OnClickClose);
        }

        RefreshDefaultContent();
    }

    protected override void OnShow()
    {
        base.OnShow();
    }

    protected override void OnDestroyPanel()
    {
        if (btnClose != null)
        {
            btnClose.onClick.RemoveListener(OnClickClose);
        }

        base.OnDestroyPanel();
    }

    private void RefreshDefaultContent()
    {
        if (txtTitle != null)
        {
            txtTitle.text = "关于游戏";
        }

        if (txtContent != null)
        {
            txtContent.text =
                "UnityDemo_MMORPG\n\n" +
                "版本：Demo v0.1\n" +
                "作者：YIMO691\n\n" +
                "这是一个基于 Unity 的 MMORPG Demo 项目。\n" +
                "当前已实现：\n" +
                "• UIFramework\n" +
                "• EventBus\n" +
                "• 角色创建流程\n" +
                "• 职业配置系统\n" +
                "• 主界面骨架\n\n" +
                "后续将继续开发：\n" +
                "• 技能系统\n" +
                "• 战斗系统\n" +
                "• 背包系统\n" +
                "• 玩家运行时数据系统";
        }
    }

    public void SetContent(string title, string content)
    {
        if (txtTitle != null)
        {
            txtTitle.text = title;
        }

        if (txtContent != null)
        {
            txtContent.text = content;
        }
    }

    private void OnClickClose()
    {
        UIManager.Instance.HidePanel<AboutPanel>();
    }
}
