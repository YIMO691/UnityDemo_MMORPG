using UnityEngine;
using UnityEngine.EventSystems;

public class MapClickReceiver : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private RectTransform mapImageRect;
    [SerializeField] private float sampleHeight = 10f;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (mapImageRect == null) return;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(mapImageRect, eventData.position, eventData.pressEventCamera, out var localPoint)) return;
        Rect rect = mapImageRect.rect;
        float percentX = Mathf.InverseLerp(rect.xMin, rect.xMax, localPoint.x);
        float percentY = Mathf.InverseLerp(rect.yMin, rect.yMax, localPoint.y);
        var config = MapService.Instance.GetCurrentMapConfig();
        if (config == null) return;
        float worldX = Mathf.Lerp(config.worldMinX, config.worldMaxX, percentX);
        float worldZ = Mathf.Lerp(config.worldMinZ, config.worldMaxZ, percentY);
        Vector3 targetWorldPos = new Vector3(worldX, sampleHeight, worldZ);
        EventBus.Publish(new NavigationMoveRequestEvent(new NavigationMoveRequest(NavigationConsts.PlayerAgentId, targetWorldPos, 0.25f)));
    }
}
