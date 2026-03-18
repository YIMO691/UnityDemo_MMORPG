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

        PlayerData playerData = GamePlayerDataService.Instance.GetCurrentPlayerData();
        if (playerData == null || playerData.runtimeData == null)
        {
            txtCoord.text = "坐标: (0, 0, 0)";
            return;
        }

        float x = playerData.runtimeData.posX;
        float y = playerData.runtimeData.posY;
        float z = playerData.runtimeData.posZ;

        txtCoord.text = $"坐标: ({x:F1}, {y:F1}, {z:F1})";
    }

    private Vector2 WorldToMapPosition(Vector3 worldPos, MapConfig config)
    {
        float xPercent = (worldPos.x - config.worldMinX) / (config.worldMaxX - config.worldMinX);
        float zPercent = (worldPos.z - config.worldMinZ) / (config.worldMaxZ - config.worldMinZ);
        xPercent = Mathf.Clamp01(xPercent);
        zPercent = Mathf.Clamp01(zPercent);
        float width = mapImageRect != null ? mapImageRect.rect.width : 0f;
        float height = mapImageRect != null ? mapImageRect.rect.height : 0f;
        float x = xPercent * width;
        float y = zPercent * height;
        return new Vector2(x, y);
    }

    private void RefreshPlayerMarker()
    {
        if (playerMarker == null || mapImageRect == null)
            return;

        MapConfig config = MapService.Instance.GetCurrentMapConfig();
        Vector3 worldPos = MapService.Instance.GetPlayerPosition();

        if (config == null)
        {
            playerMarker.gameObject.SetActive(false);
            return;
        }

        Vector2 pos = WorldToMapPosition(worldPos, config);
        playerMarker.gameObject.SetActive(true);
        playerMarker.anchoredPosition = pos;
    }
}
