public struct OpenItemDetailPopupEvent
{
    public ItemDetailContext context;

    public OpenItemDetailPopupEvent(ItemDetailContext context)
    {
        this.context = context;
    }
}
