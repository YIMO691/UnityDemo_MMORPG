using TMPro;
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
    [SerializeField] private Image imgRoleHead;

    [Header("MenuBar")]
    [SerializeField] private Button btnBag;
    [SerializeField] private Button btnMap;
    [SerializeField] private Button btnRoleDetail;
    [SerializeField] private Button btnSetting;

    [Header("MiniMap")]
    [SerializeField] private RawImage imgMiniMap;


    private PlayerData currentPlayerData;

    protected override void OnCreate()
    {
        BindButtons();
    }

    protected override void OnShow()
    {
        base.OnShow();
        RefreshView();
        EventBus.Subscribe<PlayerHpChangedEvent>(OnHpChanged);
        EventBus.Subscribe<PlayerStaminaChangedEvent>(OnStaminaChanged);
    }

    protected override void OnHide()
    {
        EventBus.Unsubscribe<PlayerHpChangedEvent>(OnHpChanged);
        EventBus.Unsubscribe<PlayerStaminaChangedEvent>(OnStaminaChanged);
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

        if (imgRoleHead != null)
            imgRoleHead.sprite = null;
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
        int maxStamina = currentPlayerData.attributeData.maxStamina;
        int currentStamina = currentPlayerData.runtimeData.currentStamina;

        if (staminaFill == null || maxStamina <= 0)
        {
            if (staminaFill != null) staminaFill.fillAmount = 0f;
            return;
        }

        float percent = (float)currentStamina / maxStamina;
        staminaFill.fillAmount = Mathf.Clamp01(percent);
    }

    private void OnHpChanged(PlayerHpChangedEvent e)
    {
        Debug.Log($"[MainPanel] HP changed: {e.currentHp}/{e.maxHp}");
        if (hpFill == null) return;
        if (e == null || e.maxHp <= 0)
        {
            hpFill.fillAmount = 0f;
            return;
        }
        float percent = (float)e.currentHp / e.maxHp;
        hpFill.fillAmount = Mathf.Clamp01(percent);
    }

    private void OnStaminaChanged(PlayerStaminaChangedEvent e)
    {
        //Debug.Log($"[MainPanel] Stamina changed: {e.currentStamina}/{e.maxStamina}");
        if (staminaFill == null) return;
        if (e == null || e.maxStamina <= 0)
        {
            staminaFill.fillAmount = 0f;
            return;
        }
        float percent = (float)e.currentStamina / e.maxStamina;
        staminaFill.fillAmount = Mathf.Clamp01(percent);
    }


    private void SetBarsEmpty()
    {
        if (hpFill != null) hpFill.fillAmount = 0f;
        if (staminaFill != null) staminaFill.fillAmount = 0f;
    }

    private void RefreshPortrait()
    {
        if (imgRoleHead == null)
            return;

        if (currentPlayerData == null || currentPlayerData.baseData == null)
        {
            imgRoleHead.sprite = null;
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
            Debug.LogWarning("[MainPanel] 职业配置不存在，classId = " + currentPlayerData.baseData.classId);
            imgRoleHead.sprite = null;
            return;
        }

        if (string.IsNullOrEmpty(config.mainHeadId))
        {
            Debug.LogWarning("[MainPanel] mainHeadId 为空，无法加载主界面头像。");
            imgRoleHead.sprite = null;
            return;
        }

        string path = AssetPaths.PortraitRoleHeadRoot + config.mainHeadId;
        Sprite sp = ResourceManager.Instance.Load<Sprite>(path);

        if (sp == null)
        {
            Debug.LogWarning("[MainPanel] 主界面头像资源加载失败：" + path);
            sp = ResourceManager.Instance.Load<Sprite>(AssetPaths.PortraitRoleHeadRoot + "default_head");
        }

        imgRoleHead.sprite = sp;
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
        EventBus.Publish(new OpenInventoryPanelEvent());
    }

    private void OnClickMap()
    {
        EventBus.Publish(new OpenPanelEvent(UIRouteNames.MapPanel));
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
