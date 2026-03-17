public static class UIDialogService
{
    public static void ShowConfirm(string message, System.Action onConfirm, System.Action onCancel = null)
    {
        UIManager.Instance.ShowPanel(UIRouteNames.ConfirmPanel);
        BasePanel panel = UIManager.Instance.GetPanel(UIRouteNames.ConfirmPanel);
        if (panel == null) return;

        var method = panel.GetType().GetMethod("SetData");
        if (method == null) return;
        method.Invoke(panel, new object[] { message, onConfirm, onCancel });
    }
}
