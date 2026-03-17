public static class UIDialogService
{
    public static void ShowConfirm(string message, System.Action onConfirm, System.Action onCancel = null)
    {
        UIManager.Instance.ShowPanel<ConfirmPanel>();
        ConfirmPanel panel = UIManager.Instance.GetPanel<ConfirmPanel>();
        if (panel != null)
        {
            panel.SetData(message, onConfirm, onCancel);
        }
    }
}

