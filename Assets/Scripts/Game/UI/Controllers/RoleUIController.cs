public class RoleUIController
{
    private static readonly RoleUIController instance = new RoleUIController();
    public static RoleUIController Instance => instance;

    private bool isInited = false;

    private RoleUIController() { }

    public void Init()
    {
        if (isInited) return;
        EventBus.Subscribe<OpenRoleInfoPanelEvent>(OnOpenRoleInfoPanelEvent);
        isInited = true;
    }

    public void Clear()
    {
        if (!isInited) return;
        EventBus.Unsubscribe<OpenRoleInfoPanelEvent>(OnOpenRoleInfoPanelEvent);
        isInited = false;
    }

    private void OnOpenRoleInfoPanelEvent(OpenRoleInfoPanelEvent e)
    {
        RoleInfoPanel panel = UIManager.Instance.ShowPanel<RoleInfoPanel>();
        if (panel != null)
        {
            panel.SetRoleInfo(e.Config);
        }
    }
}
