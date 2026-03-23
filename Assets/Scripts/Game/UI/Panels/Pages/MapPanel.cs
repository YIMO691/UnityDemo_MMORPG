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
    [SerializeField] private bool enablePathPoolDebugLog = false;
    private readonly List<GameObject> activePathSegments = new List<GameObject>();

    protected override void OnCreate()
    {
        if (btnClose != null)
            btnClose.onClick.AddListener(OnClickClose);
        if (pathRoot != null)
        {
            var img = pathRoot.GetComponent<Image>();
            if (img != null)
            {
                img.enabled = false;
                img.raycastTarget = false;
            }
        }
        // 注册路径段对象池（使用面板指定的 prefab）
        if (pathSegmentPrefab != null && !PoolManager.Instance.Contains(PoolKey.MapPathSegment))
        {
            PoolManager.Instance.RegisterPool(PoolKey.MapPathSegment, pathSegmentPrefab.gameObject, 50);
        }
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

        RefreshPlayerCoord();
        RefreshPlayerMarker();
        RefreshDestinationMarker();
        RefreshPathVisual();
    }

    protected override void OnHide()
    {
        ClearPathVisual();
        base.OnHide();
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
        //if (enablePathPoolDebugLog)
        //{
        //    Debug.Log($"[MapPanel] ClearPathVisual before activeCount={activePathSegments.Count}");
        //}
        for (int i = 0; i < activePathSegments.Count; i++)
        {
            var go = activePathSegments[i];
            if (go != null) PoolManager.Instance.Recycle(go);
        }
        activePathSegments.Clear();
        pathSegments.Clear();
        //if (enablePathPoolDebugLog)
        //{
        //    Debug.Log($"[MapPanel] ClearPathVisual after activeCount={activePathSegments.Count}");
        //    //Debug.Log(PoolManager.Instance.GetPoolDebugInfo(PoolKey.MapPathSegment));
        //}
    }

    private void DrawPathSegment(Vector2 from, Vector2 to, Color color)
    {
        if (pathRoot == null || pathSegmentPrefab == null)
        {
            Debug.LogWarning("[MapPanel] DrawPathSegment 失败，pathRoot 或 pathSegmentPrefab 为空。");
            return;
        }

        GameObject go = PoolManager.Instance.Spawn(PoolKey.MapPathSegment, pathRoot);
        if (go == null) return;
        Image seg = go.GetComponent<Image>();
        if (seg == null) seg = go.AddComponent<Image>();
        seg.raycastTarget = false;
        if (seg.sprite == null)
        {
            Debug.LogWarning("[MapPanel] pathSegmentPrefab 未绑定 Sprite，无法绘制路径。");
            PoolManager.Instance.Recycle(go);
            return;
        }
        seg.type = Image.Type.Simple;
        // 默认色透明度防御
        if (color.a <= 0f)
        {
            color.a = 1f;
        }
        seg.color = color;

        go.transform.SetAsLastSibling();

        RectTransform rt = go.GetComponent<RectTransform>();

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

        //Debug.Log($"[MapPanel] 已创建线段，length={length}, pos={rt.anchoredPosition}, angle={angle}");

        activePathSegments.Add(go);
        //if (enablePathPoolDebugLog)
        //{
        //    Debug.Log($"[MapPanel] DrawPathSegment activeCount={activePathSegments.Count}, len={length:F2}");
        //}
    }

   
    private void RefreshPathVisual()
    {
        //if (enablePathPoolDebugLog)
        //{
        //    Debug.Log("[MapPanel] RefreshPathVisual begin");
        //    Debug.Log(PoolManager.Instance.GetPoolDebugInfo(PoolKey.MapPathSegment));
        //}
        ClearPathVisual();
        PlayerNavigator navigator = GetPlayerNavigator();
        if (navigator == null || !navigator.HasPath())
        {
            if (enablePathPoolDebugLog)
            {
                Debug.Log("[MapPanel] 无可绘制路径");
                Debug.Log(PoolManager.Instance.GetPoolDebugInfo(PoolKey.MapPathSegment));
            }
            return;
        }
        Vector3[] points = navigator.GetPathPoints();
        int startIndex = navigator.GetCurrentPathIndex();
        if (points == null || points.Length < 2)
        {
            if (enablePathPoolDebugLog)
            {
                Debug.Log("[MapPanel] 路径点不足");
                Debug.Log(PoolManager.Instance.GetPoolDebugInfo(PoolKey.MapPathSegment));
            }
            return;
        }
        MapConfig config = MapService.Instance.GetCurrentMapConfig();
        if (config == null) return;
        Vector3 playerWorldPos = GetCurrentPlayerWorldPosition();
        Vector2 lastUiPos = WorldToMapPosition(playerWorldPos, config);
        Color color = reachablePathColor == default ? Color.green : reachablePathColor;
        for (int i = startIndex; i < points.Length; i++)
        {
            Vector2 nextUiPos = WorldToMapPosition(points[i], config);
            DrawPathSegment(lastUiPos, nextUiPos, color);
            lastUiPos = nextUiPos;
        }
        //if (enablePathPoolDebugLog)
        //{
        //    Debug.Log($"[MapPanel] RefreshPathVisual end activeCount={activePathSegments.Count}");
        //    Debug.Log(PoolManager.Instance.GetPoolDebugInfo(PoolKey.MapPathSegment));
        //}
    }

    private void RefreshDestinationMarker()
    {
        if (destinationMarker == null || mapImageRect == null)
            return;

        var state = NavigationService.Instance.GetPlayerVisualState();

        // 没有有效目标时隐藏
        if (state.destination == Vector3.zero)
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

        Vector3 playerWorldPos = GetCurrentPlayerWorldPosition();

        // 只有“可达且已接近终点”时才隐藏
        if (state.isReachable && (playerWorldPos - state.destination).sqrMagnitude < 1f)
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
            img.color = state.isReachable
                ? (reachableDestinationColor == default ? Color.green : reachableDestinationColor)
                : (unreachableDestinationColor == default ? Color.red : unreachableDestinationColor);
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
