using UnityEngine;
using UnityEngine.UI;

public class UIMask : MonoBehaviour
{
    [SerializeField] private Button maskButton;
    [SerializeField] private CanvasGroup canvasGroup;

    private void Awake()
    {
        if (maskButton == null)
            maskButton = GetComponent<Button>();

        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        maskButton.onClick.AddListener(OnClickMask);

        Hide();
    }

    private void OnDestroy()
    {
        if (maskButton != null)
            maskButton.onClick.RemoveListener(OnClickMask);
    }

    private void OnClickMask()
    {
        UIManager.Instance.OnMaskClicked();
    }

    public void Show()
    {
        gameObject.SetActive(true);

        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }

    public void Hide()
    {
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        gameObject.SetActive(false);
    }
}
