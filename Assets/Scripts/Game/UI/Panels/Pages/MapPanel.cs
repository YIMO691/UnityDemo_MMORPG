using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

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
    [SerializeField] private RectTransform destinationMarker;

    [Header("Path Visual")]
    [SerializeField] private RectTransform pathRoot;
    [SerializeField] private Image pathSegmentPrefab;
    [SerializeField] private float pathThickness = 6f;
    [SerializeField] private Color reachablePathColor = default;
    [SerializeField] private Color unreachablePathColor = default;
    [SerializeField] private Color reachableDestinationColor = default;
    [SerializeField] private Color unreachableDestinationColor = default;
    private readonly List<RectTransform> pathSegments = new List<RectTransform>();

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

    protected override void Update()
{
    base.Update();

    if (!IsVisible)
        return;

    RefreshPlayerMarker();
    RefreshDestinationMarker();
    RefreshPathVisual();
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

    private PlayerNavigator GetPlayerNavigator()
    {
        Transform t = PlayerLocator.Instance.GetPlayerTransform();
        if (t == null) return null;
        return t.GetComponent<PlayerNavigator>();
    }

    private void ClearPathVisual()
    {
        for (int i = 0; i < pathSegments.Count; i++)
        {
            if (pathSegments[i] != null) Destroy(pathSegments[i].gameObject);
        }
        pathSegments.Clear();
    }

    private void DrawPathSegment(Vector2 from, Vector2 to, Color color)
    {
        if (pathRoot == null || pathSegmentPrefab == null)
        {
            Debug.LogWarning("[MapPanel] DrawPathSegment 失败，pathRoot 或 pathSegmentPrefab 为空。");
            return;
        }

        Image seg = Instantiate(pathSegmentPrefab, pathRoot);
        seg.gameObject.SetActive(true);
        // 确保有可渲染的 Sprite（Image 没有 Sprite 会不渲染）
        if (seg.sprite == null)
        {
            var builtin = Resources.GetBuiltinResource<Sprite>("UI/Skin/UISprite.psd");
            seg.sprite = builtin;
        }
        seg.type = Image.Type.Simple;
        // 默认色透明度防御
        if (color.a <= 0f)
        {
            color.a = 1f;
        }
        seg.color = color;

        seg.transform.SetAsLastSibling();

        RectTransform rt = seg.rectTransform;

        Vector2 dir = to - from;
        float length = dir.magnitude;

        rt.anchorMin = new Vector2(0f, 0f);
        rt.anchorMax = new Vector2(0f, 0f);
        rt.pivot = new Vector2(0.5f, 0.5f);

        rt.anchoredPosition = (from + to) * 0.5f;
        float thickness = pathThickness > 0f ? pathThickness : 6f;
        rt.sizeDelta = new Vector2(length, thickness);

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        rt.localRotation = Quaternion.Euler(0f, 0f, angle);

        pathSegments.Add(rt);

        Debug.Log($"[MapPanel] 已创建线段，length={length}, pos={rt.anchoredPosition}, angle={angle}");
    }


    private void RefreshPathVisual()
    {
        ClearPathVisual();
        var state = NavigationService.Instance.GetPlayerVisualState();

        Debug.Log($"[MapPanel] RefreshPathVisual -> hasPath={state.hasPath}, isReachable={state.isReachable}, " +
             $"pathPointsNull={state.pathPoints == null}, " +
             $"pathCount={(state.pathPoints == null ? 0 : state.pathPoints.Length)}");

        if (!state.hasPath || !state.isReachable || state.pathPoints == null || state.pathPoints.Length == 0) return;

        MapConfig config = MapService.Instance.GetCurrentMapConfig();
        if (config == null) return;
        Vector3 playerWorldPos = GetCurrentPlayerWorldPosition();
        Vector2 lastUiPos = WorldToMapPosition(playerWorldPos, config);
        Color color = reachablePathColor == default ? Color.green : reachablePathColor;
        for (int i = 0; i < state.pathPoints.Length; i++)
        {
            Vector2 nextUiPos = WorldToMapPosition(state.pathPoints[i], config);
            DrawPathSegment(lastUiPos, nextUiPos, color);
            lastUiPos = nextUiPos;
        }
    }
        
    private void RefreshDestinationMarker()
    {
        if (destinationMarker == null || mapImageRect == null) return;
        var state = NavigationService.Instance.GetPlayerVisualState();
        if (!state.hasPath && state.destination == Vector3.zero)
        {
            destinationMarker.gameObject.SetActive(false);
            return;
        }
        MapConfig config = MapService.Instance.GetCurrentMapConfig();
        if (config == null)
        {
            destinationMarker.gameObject.SetActive(false);
            return;
        }
        Vector2 uiPos = WorldToMapPosition(state.destination, config);
        destinationMarker.gameObject.SetActive(true);
        destinationMarker.anchoredPosition = uiPos;
        var img = destinationMarker.GetComponent<Image>();
        if (img != null)
        {
            if (state.isReachable)
                img.color = reachableDestinationColor == default ? Color.green : reachableDestinationColor;
            else
                img.color = unreachableDestinationColor == default ? Color.red : unreachableDestinationColor;
        }
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
