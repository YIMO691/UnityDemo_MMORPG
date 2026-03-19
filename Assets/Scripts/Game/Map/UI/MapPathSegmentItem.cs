using UnityEngine;
using UnityEngine.UI;

public class MapPathSegmentItem : MonoBehaviour, IPoolable
{
    [SerializeField] private RectTransform rectTransform;
    [SerializeField] private Image image;

    private void Reset()
    {
        rectTransform = GetComponent<RectTransform>();
        image = GetComponent<Image>();
    }

    public void OnSpawnFromPool()
    {
        if (rectTransform != null)
        {
            rectTransform.localScale = Vector3.one;
            rectTransform.localRotation = Quaternion.identity;
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.sizeDelta = new Vector2(0f, rectTransform.sizeDelta.y);
        }
        if (image != null)
        {
            image.color = Color.white;
            image.raycastTarget = false;
        }
    }

    public void OnRecycleToPool()
    {
        if (rectTransform != null)
        {
            rectTransform.localScale = Vector3.one;
            rectTransform.localRotation = Quaternion.identity;
            rectTransform.anchoredPosition = Vector2.zero;
        }
        if (image != null)
        {
            image.color = Color.white;
            image.raycastTarget = false;
        }
    }
}
