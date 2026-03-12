public struct OpenMainPageEvent
{
    public string PanelName;
    public bool HideOld;
    public bool UseFade;

    public OpenMainPageEvent(string panelName, bool hideOld = true, bool useFade = false)
    {
        PanelName = panelName;
        HideOld = hideOld;
        UseFade = useFade;
    }
}
