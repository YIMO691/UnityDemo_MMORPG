public class InventoryUIController
{
    private static readonly InventoryUIController instance = new InventoryUIController();
    public static InventoryUIController Instance => instance;

    private bool isInited = false;

    private InventoryUIController() { }

    public void Init()
    {
        if (isInited) return;
        EventBus.Subscribe<OpenInventoryPanelEvent>(OnOpenInventoryPanelEvent);
        isInited = true;
    }

    public void Clear()
    {
        if (!isInited) return;
        EventBus.Unsubscribe<OpenInventoryPanelEvent>(OnOpenInventoryPanelEvent);
        isInited = false;
    }

    private void OnOpenInventoryPanelEvent(OpenInventoryPanelEvent e)
    {
        InventoryPanel panel = UIManager.Instance.ShowPanel<InventoryPanel>();
        if (panel != null)
        {
            panel.Refresh();
        }
    }
}
