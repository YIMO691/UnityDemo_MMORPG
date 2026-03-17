using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ConfirmPanel : BasePanel
{
    public override UILayer Layer => UILayer.Popup;
    public override bool UseMask => true;
    public override bool CloseByMask => true;
    public override UIPanelCacheMode CacheMode => UIPanelCacheMode.DestroyOnClose;

    [Header("UI")]
    [SerializeField] private Text txtMessage;
    [SerializeField] private Button btnClose;
    [SerializeField] private Button btnConfirm;
    [SerializeField] private Button btnCancel;

    private Action onConfirm;
    private Action onCancel;

    protected override void OnCreate()
    {
        if (btnClose != null) btnClose.onClick.AddListener(OnClickClose);
        if (btnConfirm != null) btnConfirm.onClick.AddListener(OnClickConfirm);
        if (btnCancel != null) btnCancel.onClick.AddListener(OnClickCancel);
    }

    protected override void OnDestroyPanel()
    {
        if (btnClose != null) btnClose.onClick.RemoveListener(OnClickClose);
        if (btnConfirm != null) btnConfirm.onClick.RemoveListener(OnClickConfirm);
        if (btnCancel != null) btnCancel.onClick.RemoveListener(OnClickCancel);

        base.OnDestroyPanel();
    }

    public void SetData(string message, Action onConfirm, Action onCancel = null)
    {
        if (txtMessage != null)
        {
            txtMessage.text = message;
        }

        this.onConfirm = onConfirm;
        this.onCancel = onCancel;
    }

    private void OnClickConfirm()
    {
        Action callback = onConfirm;
        ClearCallback();
        UIManager.Instance.HidePanel<ConfirmPanel>(useFade: false);
        callback?.Invoke();
    }

    private void OnClickCancel()
    {
        Action callback = onCancel;
        ClearCallback();
        UIManager.Instance.HidePanel<ConfirmPanel>(useFade: false);
        callback?.Invoke();
    }

    private void OnClickClose()
    {
        Action callback = onCancel;
        ClearCallback();
        UIManager.Instance.HidePanel<ConfirmPanel>(useFade: false);
        callback?.Invoke();
    }

    private void ClearCallback()
    {
        onConfirm = null;
        onCancel = null;
    }

    protected override void OnHide()
    {
        base.OnHide();
        ClearCallback();
    }
}
