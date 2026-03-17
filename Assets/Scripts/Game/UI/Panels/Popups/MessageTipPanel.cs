using UnityEngine;
using UnityEngine.UI;

public class MessageTipPanel : BasePanel
{
    public override UILayer Layer => UILayer.Popup;
    public override bool UseMask => true;
    public override bool CloseByMask => true;

    [SerializeField] private Text txtMessage;
    [SerializeField] private Button btnOk;

    protected override void OnCreate()
    {
        if (btnOk != null)
        {
            btnOk.onClick.AddListener(OnClickOk);
        }
    }

    protected override void OnDestroyPanel()
    {
        if (btnOk != null)
        {
            btnOk.onClick.RemoveListener(OnClickOk);
        }

        base.OnDestroyPanel();
    }

    public void SetMessage(string message)
    {
        if (txtMessage != null)
        {
            txtMessage.text = message;
        }
    }

    private void OnClickOk()
    {
        UIManager.Instance.HidePanel<MessageTipPanel>();
    }
}
