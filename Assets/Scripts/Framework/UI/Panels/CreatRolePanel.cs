using System.Collections.Generic;
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
    public InputField inputName;

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
        if (classConfigs == null || classConfigs.Count == 0)
        {
            Debug.LogWarning("[CreateRolePanel] 没有职业配置。");
            return;
        }

        string roleName = inputName.text.Trim();
        if (string.IsNullOrEmpty(roleName))
        {
            Debug.LogWarning("[CreateRolePanel] 请输入昵称。");
            return;
        }

        RoleClassConfig config = classConfigs[currentClassIndex];

        CreateRoleRequest request = new CreateRoleRequest
        {
            playerName = roleName,
            classId = config.id,
            genderId = 0,
            appearanceId = 0
        };

        EventBus.Publish(new CreateRoleRequestEvent(request));
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

        Sprite sp = ResourceManager.Instance.Load<Sprite>("Portrait/" + config.defaultPortraitId);
        imgCharacterPreview.sprite = sp;
    }
}
