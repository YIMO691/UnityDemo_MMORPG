using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MapPanel : BasePanel
{
    public override UILayer Layer => UILayer.Popup;
    public override bool CloseByMask => true;
    public override UIPanelCacheMode CacheMode => UIPanelCacheMode.CacheAndHide;

    [Header("Top")]
    [SerializeField] private Button btnClose;
    [SerializeField] private Text txtTitle;

    [Header("Map")]
    [SerializeField] private Image imgMap;
    [SerializeField] private Text txtMapName;
    [SerializeField] private Text txtCoord;

    [Header("Optional")]
    [SerializeField] private RectTransform playerMarker;
    [SerializeField] private RectTransform mapImageRect;

    // 使用 MapConfig 的 worldMin/Max 驱动映射（不再写死 160x160）

    protected override void OnCreate()
    {
        if (btnClose != null)
            btnClose.onClick.AddListener(OnClickClose);
    }

    protected override void OnShow()
    {
        Refresh();
    }

    protected override void OnRefresh()
    {
        RefreshMapInfo();
        RefreshPlayerCoord();
        RefreshPlayerMarker();
    }

    protected override void OnDestroyPanel()
    {
        if (btnClose != null)
            btnClose.onClick.RemoveListener(OnClickClose);
    }

    private void OnClickClose()
    {
        UIManager.Instance.HidePanel<MapPanel>();
    }

    private void Refresh()
    {
        RefreshMapInfo();
        RefreshPlayerCoord();
        RefreshPlayerMarker();
    }

    private void RefreshMapInfo()
    {
        if (txtTitle != null)
            txtTitle.text = "地图";

        MapConfig config = null;

        // 这里假设你已经有 MapDataManager / MapService
        // 若没有，我在文末列了最小需要的接口
        if (MapService.Instance != null)
            config = MapService.Instance.GetCurrentMapConfig();

        if (config == null)
        {
            if (txtMapName != null)
                txtMapName.text = "未知地图";

            if (imgMap != null)
                imgMap.sprite = null;

            return;
        }

        if (txtMapName != null)
            txtMapName.text = config.displayName;

        if (imgMap != null)
        {
            Sprite sp = ResourceManager.Instance.Load<Sprite>(UIPaths.MapImageRoot + config.mapImage);
            imgMap.sprite = sp;

            if (sp == null)
            {
                Debug.LogWarning("[MapPanel] 地图图片加载失败: " + UIPaths.MapImageRoot + config.mapImage);
            }
        }
    }

    private void RefreshPlayerCoord()
    {
        if (txtCoord == null)
            return;

        Vector3 worldPos = GetCurrentPlayerWorldPosition();
        txtCoord.text = $"坐标: ({worldPos.x:F1}, {worldPos.z:F1})";
    }

    private Vector2 WorldToMapPosition(Vector3 worldPos, MapConfig config)
    {
        if (mapImageRect == null || config == null) return Vector2.zero;

        float widthWorld = Mathf.Max(0.0001f, (config.worldMaxX - config.worldMinX));
        float heightWorld = Mathf.Max(0.0001f, (config.worldMaxZ - config.worldMinZ));

        float xPercent = (worldPos.x - config.worldMinX) / widthWorld;
        float zPercent = (worldPos.z - config.worldMinZ) / heightWorld;
        xPercent = Mathf.Clamp01(xPercent);
        zPercent = Mathf.Clamp01(zPercent);

        float width = mapImageRect.rect.width;
        float height = mapImageRect.rect.height;

        float uiX = xPercent * width;
        float uiY = zPercent * height;
        return new Vector2(uiX, uiY);
    }

    private void RefreshPlayerMarker()
    {
        if (playerMarker == null || mapImageRect == null)
            return;

        MapConfig config = MapService.Instance.GetCurrentMapConfig();
        if (config == null)
        {
            playerMarker.gameObject.SetActive(false);
            return;
        }

        Vector3 worldPos = GetCurrentPlayerWorldPosition();
        Vector2 pos = WorldToMapPosition(worldPos, config);
        playerMarker.gameObject.SetActive(true);
        playerMarker.anchoredPosition = pos;
    }

    private Vector3 GetCurrentPlayerWorldPosition()
    {
        Transform t = PlayerLocator.Instance.GetPlayerTransform();
        if (t != null) return t.position;

        PlayerData data = GamePlayerDataService.Instance.GetCurrentPlayerData();
        if (data != null && data.runtimeData != null)
        {
            return new Vector3(
                data.runtimeData.posX,
                data.runtimeData.posY,
                data.runtimeData.posZ
            );
        }
        return Vector3.zero;
    }
}
