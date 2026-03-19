public static class UIMainPages
{
    public static bool IsMainPage(string panelName)
    {
        return panelName == UIRouteNames.BeginPanel
            || panelName == UIRouteNames.CreateRolePanel
            || panelName == UIRouteNames.SettingPanel
            || panelName == UIRouteNames.ContinuePanel
            || panelName == UIRouteNames.MainPanel;
    }
}
