public class ItemDetailUIController
{
    private static readonly ItemDetailUIController instance = new ItemDetailUIController();
    public static ItemDetailUIController Instance => instance;

    private bool isInited = false;

    private ItemDetailUIController() { }

    public void Init()
    {
        if (isInited) return;
        EventBus.Subscribe<OpenItemDetailPopupEvent>(OnOpenItemDetailPopupEvent);
        isInited = true;
    }

    public void Clear()
    {
        if (!isInited) return;
        EventBus.Unsubscribe<OpenItemDetailPopupEvent>(OnOpenItemDetailPopupEvent);
        isInited = false;
    }

    private void OnOpenItemDetailPopupEvent(OpenItemDetailPopupEvent e)
    {
        ItemDetailPopup panel = UIManager.Instance.ShowPanel<ItemDetailPopup>();
        if (panel != null)
        {
            panel.SetData(e.context);
            panel.Refresh();
        }
    }
}
