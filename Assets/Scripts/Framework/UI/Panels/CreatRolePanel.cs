using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CreateRolePanel : BasePanel
{
    public override UILayer Layer => UILayer.Normal;

    [Header("Buttons")]
    public Button btnPrev;
    public Button btnNext;
    public Button btnRoleInfo;
    public Button btnBack;
    public Button btnCreate;

    [Header("Texts")]
    public Text txtRoleName;

    [Header("Input")]
    [SerializeField] private TMP_InputField inputName;

    [Header("Preview")]
    public Image imgCharacterPreview;

    private List<RoleClassConfig> classConfigs = new List<RoleClassConfig>();
    private int currentClassIndex = 0;

    protected override void OnCreate()
    {
        btnPrev.onClick.AddListener(OnClickPrev);
        btnNext.onClick.AddListener(OnClickNext);
        btnRoleInfo.onClick.AddListener(OnClickRoleInfo);
        btnBack.onClick.AddListener(OnClickBack);
        btnCreate.onClick.AddListener(OnClickCreate);
    }

    protected override void OnShow()
    {
        RoleDataManager.Instance.Init();
        classConfigs = RoleDataManager.Instance.GetAllClassConfigs();

        currentClassIndex = 0;

        if (inputName != null)
            inputName.text = string.Empty;

        RefreshRoleDisplay();
    }

    protected override void OnDestroyPanel()
    {
        btnPrev.onClick.RemoveListener(OnClickPrev);
        btnNext.onClick.RemoveListener(OnClickNext);
        btnRoleInfo.onClick.RemoveListener(OnClickRoleInfo);
        btnBack.onClick.RemoveListener(OnClickBack);
        btnCreate.onClick.RemoveListener(OnClickCreate);
    }

    private void OnClickPrev()
    {
        if (classConfigs == null || classConfigs.Count == 0)
            return;

        currentClassIndex--;
        if (currentClassIndex < 0)
            currentClassIndex = classConfigs.Count - 1;

        RefreshRoleDisplay();
    }

    private void OnClickNext()
    {
        if (classConfigs == null || classConfigs.Count == 0)
            return;

        currentClassIndex++;
        if (currentClassIndex >= classConfigs.Count)
            currentClassIndex = 0;

        RefreshRoleDisplay();
    }

    private void OnClickRoleInfo()
    {
        if (classConfigs == null || classConfigs.Count == 0)
            return;

        RoleClassConfig config = classConfigs[currentClassIndex];
        EventBus.Publish(new OpenRoleInfoPanelEvent(config));
    }


    private void OnClickBack()
    {
        Debug.Log("[CreateRolePanel] 返回主菜单");

        UIManager.Instance.HidePanel<CreateRolePanel>(false);

        CameraEvent cameraEvent = Camera.main.GetComponent<CameraEvent>();

        if (cameraEvent != null)
        {
            cameraEvent.ResetCamera();
        }

        BeginPanel panel = UIManager.Instance.ShowMainPage<BeginPanel>(false, false);

        if (panel != null)
        {
            panel.ResetPanelState();
        }
    }


    private void OnClickCreate()
    {
        if (inputName == null)
        {
            Debug.LogError("[CreateRolePanel] inputName 未绑定。");
            return;
        }

        string playerName = inputName.text.Trim();
        string className = (classConfigs != null && classConfigs.Count > 0 && currentClassIndex >= 0 && currentClassIndex < classConfigs.Count)
            ? classConfigs[currentClassIndex].displayName
            : "未知职业";

        if (string.IsNullOrEmpty(playerName))
        {
            if (UIManager.Instance == null)
            {
                Debug.LogError("[CreateRolePanel] UIManager.Instance 为空。");
                return;
            }

            MessageTipPanel panel = UIManager.Instance.ShowPanel<MessageTipPanel>();
            if (panel != null)
            {
                panel.SetMessage("请输入角色名称");
            }
            else
            {
                Debug.LogError("[CreateRolePanel] MessageTipPanel 打开失败，请检查 prefab 路径/类名/UIManager。");
            }

            return;
        }

        if (classConfigs == null || classConfigs.Count == 0)
        {
            Debug.LogError("[CreateRolePanel] classConfigs 为空，职业配置未加载。");
            return;
        }

        if (currentClassIndex < 0 || currentClassIndex >= classConfigs.Count)
        {
            Debug.LogError("[CreateRolePanel] currentClassIndex 越界。");
            return;
        }

        CreateRoleRequest request = new CreateRoleRequest
        {
            playerName = playerName,
            classId = classConfigs[currentClassIndex].id,
            genderId = 0,
            appearanceId = 0
        };

        string confirmMessage = $"是否创建角色？\n\n姓名：{playerName}\n职业：{className}";

        UIManager.Instance.ShowConfirm(confirmMessage, () =>
        {
            EventBus.Publish(new CreateRoleRequestEvent(request));
        });
    }


    private void RefreshRoleDisplay()
    {
        if (classConfigs == null || classConfigs.Count == 0)
        {
            txtRoleName.text = "无职业";
            if (imgCharacterPreview != null)
                imgCharacterPreview.sprite = null;
            return;
        }

        RoleClassConfig config = classConfigs[currentClassIndex];

        txtRoleName.text = config.displayName;

        RefreshPortrait(config);
    }

    private void RefreshPortrait(RoleClassConfig config)
    {
        if (imgCharacterPreview == null)
            return;

        if (string.IsNullOrEmpty(config.defaultPortraitId))
        {
            imgCharacterPreview.sprite = null;
            return;
        }

        Sprite sp = ResourceManager.Instance.Load<Sprite>(UIPaths.PortraitCreateRoleRoot + config.defaultPortraitId);
        imgCharacterPreview.sprite = sp;
    }
}
