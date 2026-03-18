using TMPro;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using UnityEngine.UI;

public class MainPanel : BasePanel
{
    public override UILayer Layer => UILayer.Normal;

    [Header("PlayerStatus")]
    [SerializeField] private TMP_Text txtPlayerLevel;
    [SerializeField] private TMP_Text txtPlayerName;

    [SerializeField] private Image hpFill;
    [SerializeField] private Image staminaFill;

    [Header("SkillBar")]
    [SerializeField] private Button btnSkill1;
    [SerializeField] private Button btnSkill2;
    [SerializeField] private Button btnSkill3;

    [Header("Portrait")]
    [SerializeField] private GameObject engineerHead;
    [SerializeField] private GameObject infantryHead;
    [SerializeField] private GameObject medicHead;
    [SerializeField] private GameObject sniperHead;

    [Header("MenuBar")]
    [SerializeField] private Button btnBag;
    [SerializeField] private Button btnMap;
    [SerializeField] private Button btnRoleDetail;
    [SerializeField] private Button btnSetting;

    private PlayerData currentPlayerData;

    protected override void OnCreate()
    {
        BindButtons();
    }

    protected override void OnShow()
    {
        base.OnShow();
        RefreshView();
    }

    protected override void OnHide()
    {
        base.OnHide();
    }

    protected override void OnDestroyPanel()
    {
        UnbindButtons();
        base.OnDestroyPanel();
    }

    private void BindButtons()
    {
        if (btnSkill1 != null) btnSkill1.onClick.AddListener(OnClickSkill1);
        if (btnSkill2 != null) btnSkill2.onClick.AddListener(OnClickSkill2);
        if (btnSkill3 != null) btnSkill3.onClick.AddListener(OnClickSkill3);

        if (btnBag != null) btnBag.onClick.AddListener(OnClickBag);
        if (btnMap != null) btnMap.onClick.AddListener(OnClickMap);
        if (btnRoleDetail != null) btnRoleDetail.onClick.AddListener(OnClickRoleDetail);
        if (btnSetting != null) btnSetting.onClick.AddListener(OnClickSetting);
    }

    private void UnbindButtons()
    {
        if (btnSkill1 != null) btnSkill1.onClick.RemoveListener(OnClickSkill1);
        if (btnSkill2 != null) btnSkill2.onClick.RemoveListener(OnClickSkill2);
        if (btnSkill3 != null) btnSkill3.onClick.RemoveListener(OnClickSkill3);

        if (btnBag != null) btnBag.onClick.RemoveListener(OnClickBag);
        if (btnMap != null) btnMap.onClick.RemoveListener(OnClickMap);
        if (btnRoleDetail != null) btnRoleDetail.onClick.RemoveListener(OnClickRoleDetail);
        if (btnSetting != null) btnSetting.onClick.RemoveListener(OnClickSetting);
    }

    private void RefreshView()
    {
        currentPlayerData = GamePlayerDataService.Instance.GetCurrentPlayerData();

        if (currentPlayerData == null)
        {
            Debug.LogWarning("[MainPanel] 当前没有玩家数据，显示默认内容。");
            RefreshEmptyView();
            return;
        }

        RefreshPlayerInfo();
        RefreshBars();
        RefreshPortrait();
    }

    private void RefreshEmptyView()
    {
        if (txtPlayerName != null)
            txtPlayerName.text = "无角色";

        if (txtPlayerLevel != null)
            txtPlayerLevel.text = "Lv.0";

        if (hpFill != null)
            hpFill.fillAmount = 0f;

        if (staminaFill != null)
            staminaFill.fillAmount = 0f;
    }

    private void RefreshPlayerInfo()
    {
        if (currentPlayerData.baseData != null)
        {
            if (txtPlayerName != null)
                txtPlayerName.text = currentPlayerData.baseData.roleName;
        }
        else
        {
            if (txtPlayerName != null)
                txtPlayerName.text = "未知角色";
        }

        if (currentPlayerData.progressData != null)
        {
            if (txtPlayerLevel != null)
                txtPlayerLevel.text = $"Lv.{currentPlayerData.progressData.level}";
        }
        else
        {
            if (txtPlayerLevel != null)
                txtPlayerLevel.text = "Lv.0";
        }
    }

    private void RefreshBars()
    {
        if (currentPlayerData == null
            || currentPlayerData.attributeData == null
            || currentPlayerData.runtimeData == null)
        {
            SetBarsEmpty();
            return;
        }

        RefreshHpBar();
        RefreshStaminaBar();
    }

    private void RefreshHpBar()
    {
        int maxHp = currentPlayerData.attributeData.maxHp;
        int currentHp = currentPlayerData.runtimeData.currentHp;

        if (hpFill == null || maxHp <= 0)
        {
            if (hpFill != null) hpFill.fillAmount = 0f;
            return;
        }

        float percent = (float)currentHp / maxHp;
        hpFill.fillAmount = Mathf.Clamp01(percent);
    }

    private void RefreshStaminaBar()
    {
        // stamina 以 MP 表示：attributeData.maxMp / runtimeData.currentMp
        int maxStamina = currentPlayerData.attributeData.maxMp;
        int currentStamina = currentPlayerData.runtimeData.currentMp;

        if (staminaFill == null || maxStamina <= 0)
        {
            if (staminaFill != null) staminaFill.fillAmount = 0f;
            return;
        }

        float percent = (float)currentStamina / maxStamina;
        staminaFill.fillAmount = Mathf.Clamp01(percent);
    }

    private void SetBarsEmpty()
    {
        if (hpFill != null) hpFill.fillAmount = 0f;
        if (staminaFill != null) staminaFill.fillAmount = 0f;
    }

    private void RefreshPortrait()
    {
        SetAllPortraitInactive();

        if (currentPlayerData == null || currentPlayerData.baseData == null)
        {
            if (infantryHead != null) infantryHead.SetActive(true);
            return;
        }

        int classId = currentPlayerData.baseData.classId;
        switch (classId)
        {
            case 1:
                if (infantryHead != null) infantryHead.SetActive(true);
                break;
            case 2:
                if (sniperHead != null) sniperHead.SetActive(true);
                break;
            case 3:
                if (medicHead != null) medicHead.SetActive(true);
                break;
            case 4:
                if (engineerHead != null) engineerHead.SetActive(true);
                break;
            default:
                if (infantryHead != null) infantryHead.SetActive(true);
                break;
        }
    }

    private void SetAllPortraitInactive()
    {
        if (engineerHead != null) engineerHead.SetActive(false);
        if (infantryHead != null) infantryHead.SetActive(false);
        if (medicHead != null) medicHead.SetActive(false);
        if (sniperHead != null) sniperHead.SetActive(false);
    }

    private void OnClickSkill1()
    {
        ShowMessage("技能1 功能开发中");
    }

    private void OnClickSkill2()
    {
        ShowMessage("技能2 功能开发中");
    }

    private void OnClickSkill3()
    {
        ShowMessage("技能3 功能开发中");
    }

    private void OnClickBag()
    {
        ShowMessage("背包功能开发中");
    }

    private void OnClickMap()
    {
        ShowMessage("地图功能开发中");
    }

    private void OnClickRoleDetail()
    {
        if (currentPlayerData == null || currentPlayerData.baseData == null)
        {
            ShowMessage("当前没有角色数据");
            return;
        }

        RoleClassConfig config = RoleDataManager.Instance.GetClassConfig(currentPlayerData.baseData.classId);
        if (config == null)
        {
            RoleDataManager.Instance.Init();
            config = RoleDataManager.Instance.GetClassConfig(currentPlayerData.baseData.classId);
        }
        if (config == null)
        {
            ShowMessage("职业配置不存在");
            return;
        }

        EventBus.Publish(new OpenRoleInfoPanelEvent(config));
    }

    private void OnClickSetting()
    {
        EventBus.Publish(new OpenPanelEvent(UIRouteNames.SettingPanel));
    }

    private void ShowMessage(string message)
    {
        UIManager.Instance.ShowPanel<MessageTipPanel>();

        MessageTipPanel panel = UIManager.Instance.GetPanel<MessageTipPanel>();
        if (panel != null)
        {
            panel.SetMessage(message);
        }
        else
        {
            Debug.LogError("[MainPanel] MessageTipPanel 获取失败：" + message);
        }
    }
}
