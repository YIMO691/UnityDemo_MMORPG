using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager
{
    private static readonly UIManager instance = new UIManager();
    public static UIManager Instance => instance;

    private readonly Dictionary<string, BasePanel> panelDic = new Dictionary<string, BasePanel>();
    private readonly Dictionary<UILayer, Transform> layerDic = new Dictionary<UILayer, Transform>();

    // Popup层的面板需要入栈，出栈时才显示下面的面板
    private readonly Stack<BasePanel> popupStack = new Stack<BasePanel>();
    private UIMask uiMask;


    private Transform canvasTrans;
    private GameObject canvasObj;
    private bool isInited = false;

    private BasePanel currentMainPage;


    private UIManager() { }

    public void Init()
    {
        if (isInited) return;

        EventBus.Subscribe<OpenPanelEvent>(OnOpenPanelEvent);
        EventBus.Subscribe<ClosePanelEvent>(OnClosePanelEvent);

        EventBus.Subscribe<OpenMainPageEvent>(OnOpenMainPageEvent);


        GameObject canvasPrefab = ResourceManager.Instance.Load<GameObject>(AssetPaths.PanelCanvas);
        if (canvasPrefab == null)
        {
            Debug.LogError($"[UIManager] PanelCanvas prefab not found: {AssetPaths.PanelCanvas}");
            return;
        }

        canvasObj = GameObject.Instantiate(canvasPrefab);
        canvasTrans = canvasObj.transform;
        GameObject.DontDestroyOnLoad(canvasObj);

        InitLayer();

        InitMask();

        isInited = true;
        Debug.Log("[UIManager] Init Success.");
    }

    public bool IsInited => isInited;

    private void InitLayer()
    {
        if (canvasTrans == null)
        {
            Debug.LogError("[UIManager] canvasTrans is null when InitLayer()");
            return;
        }

        layerDic.Clear();

        foreach (UILayer layer in Enum.GetValues(typeof(UILayer)))
        {
            Transform layerTrans = canvasTrans.Find(layer.ToString());
            if (layerTrans == null)
            {
                Debug.LogError($"[UIManager] Layer not found: {layer}");
                continue;
            }

            layerDic[layer] = layerTrans;
        }
    }

    #region popup相关功能
    private void InitMask()
    {
        GameObject maskPrefab = ResourceManager.Instance.Load<GameObject>(AssetPaths.UIMask);
        if (maskPrefab == null)
        {
            Debug.LogError($"[UIManager] UIMask prefab not found: {AssetPaths.UIMask}");
            return;
        }

        GameObject maskObj = GameObject.Instantiate(maskPrefab);

        if (!layerDic.TryGetValue(UILayer.Popup, out Transform popupLayer))
        {
            Debug.LogError("[UIManager] Popup layer not found when InitMask()");
            GameObject.Destroy(maskObj);
            return;
        }
        maskObj.transform.SetParent(popupLayer, false);

        uiMask = maskObj.GetComponent<UIMask>();
        if (uiMask == null)
        {
            Debug.LogError("[UIManager] UIMask component not found on prefab.");
            GameObject.Destroy(maskObj);
            return;
        }

        uiMask.Hide();
    }


    private void TryPushPopup(BasePanel panel)
    {
        if (panel.Layer != UILayer.Popup)
            return;

        if (popupStack.Contains(panel))
            return;

        panel.transform.SetAsLastSibling();
        popupStack.Push(panel);

        RefreshPopupMask();
    }

    private void RemovePopupFromStack(BasePanel target)
    {
        if (popupStack.Count == 0)
            return;

        Stack<BasePanel> tempStack = new Stack<BasePanel>();

        while (popupStack.Count > 0)
        {
            BasePanel panel = popupStack.Pop();
            if (panel != target)
                tempStack.Push(panel);
        }

        while (tempStack.Count > 0)
        {
            popupStack.Push(tempStack.Pop());
        }

        RefreshPopupMask();
    }

    private void RefreshPopupMask()
    {
        if (uiMask == null)
            return;

        if (popupStack.Count == 0)
        {
            uiMask.Hide();
            return;
        }

        BasePanel topPanel = popupStack.Peek();
        if (topPanel == null || !topPanel.UseMask)
        {
            uiMask.Hide();
            return;
        }

        uiMask.Show();

        topPanel.transform.SetAsLastSibling();

        int topIndex = topPanel.transform.GetSiblingIndex();
        uiMask.transform.SetSiblingIndex(Mathf.Max(0, topIndex - 1));
    }

    public void OnMaskClicked()
    {
        if (popupStack.Count == 0)
            return;

        BasePanel topPanel = popupStack.Peek();
        if (topPanel == null)
            return;

        if (!topPanel.CloseByMask)
            return;

        DestroyPanel(topPanel.GetType().Name, true);
    }
    #endregion

    private void OnOpenPanelEvent(OpenPanelEvent e)
    {
        ShowPanel(e.PanelName);
    }

    private void OnClosePanelEvent(ClosePanelEvent e)
    {
        HidePanel(e.PanelName);
    }

    private void OnOpenMainPageEvent(OpenMainPageEvent e)
    {
        ShowMainPage(e.PanelName, e.HideOld, e.UseFade);
    }

   public void ShowConfirm(string message, System.Action onConfirm, System.Action onCancel = null)
    {
        UIDialogService.ShowConfirm(message, onConfirm, onCancel);
    }


    public T ShowPanel<T>() where T : BasePanel
    {
        return ShowPanel(typeof(T).Name) as T;
    }

    public BasePanel ShowPanel(string panelName)
    {
        if (!CheckInit()) return null;

        if (panelDic.TryGetValue(panelName, out BasePanel existPanel))
        {
            TryPushPopup(existPanel);
            existPanel.ShowMe();
            Debug.Log($"[UIManager] ReShow Panel: {panelName}");
            return existPanel;
        }

        GameObject panelPrefab = ResourceManager.Instance.Load<GameObject>(AssetPaths.Window(panelName));
        if (panelPrefab == null)
        {
            Debug.LogError($"[UIManager] Panel prefab not found: {AssetPaths.Window(panelName)}");
            return null;
        }

        GameObject panelObj = GameObject.Instantiate(panelPrefab);

        BasePanel panel = panelObj.GetComponent<BasePanel>();
        if (panel == null)
        {
            Debug.LogError($"[UIManager] BasePanel component not found on prefab: {panelName}");
            GameObject.Destroy(panelObj);
            return null;
        }

        UILayer layer = panel.Layer;
        if (!layerDic.TryGetValue(layer, out Transform parent))
        {
            Debug.LogError($"[UIManager] Layer not exist: {layer}");
            GameObject.Destroy(panelObj);
            return null;
        }

        panelObj.transform.SetParent(parent, false);

        panel.Create();
        panelDic.Add(panelName, panel);

        // 先处理 Popup 栈和层级
        TryPushPopup(panel);

        // 再显示面板
        panel.ShowMe();

        Debug.Log($"[UIManager] ShowPanel Success: {panelName}");
        return panel;
    }


    public T ShowMainPage<T>(bool hideOld = true, bool useFade = false) where T : BasePanel
    {
        return ShowMainPage(typeof(T).Name, hideOld, useFade) as T;
    }

    public BasePanel ShowMainPage(string panelName, bool hideOld = true, bool useFade = false)
    {
        BasePanel newPage = ShowPanel(panelName);
        if (newPage == null)
            return null;

        if (newPage.Layer != UILayer.Normal)
        {
            Debug.LogWarning($"[UIManager] ShowMainPage called with non-Normal panel: {panelName}");
            return newPage;
        }

        if (hideOld && currentMainPage != null && currentMainPage != newPage)
        {
            HidePanel(currentMainPage.GetType().Name, useFade);
        }

        currentMainPage = newPage;
        return newPage;
    }

    public void HidePanel<T>(bool useFade = true) where T : BasePanel
    {
        HidePanel(typeof(T).Name, useFade);
    }

    public void HidePanel(string panelName, bool useFade = true)
    {
        if (!panelDic.TryGetValue(panelName, out BasePanel panel))
        {
            Debug.LogWarning($"[UIManager] HidePanel failed, panel not found: {panelName}");
            return;
        }

        if (panel.CacheMode == BasePanel.UIPanelCacheMode.DestroyOnClose)
        {
            DestroyPanel(panelName, useFade);
            return;
        }

        if (useFade)
        {
            panel.HideMe();
        }
        else
        {
            panel.gameObject.SetActive(false);
        }

        if (panel.Layer == UILayer.Popup)
            RemovePopupFromStack(panel);

        Debug.Log($"[UIManager] HidePanel Success: {panelName}");
    }

    public void DestroyPanel<T>(bool useFade = false) where T : BasePanel
    {
        DestroyPanel(typeof(T).Name, useFade);
    }

    public void DestroyPanel(string panelName, bool useFade = false)
    {
        if (!panelDic.TryGetValue(panelName, out BasePanel panel))
        {
            Debug.LogWarning($"[UIManager] DestroyPanel failed, panel not found: {panelName}");
            return;
        }

        void DoDestroy()
        {
            if (panel != null)
            {
                panel.DestroyPanel();
            }

            panelDic.Remove(panelName);
            if (currentMainPage == panel)
            {
                currentMainPage = null;
            }
            Debug.Log($"[UIManager] DestroyPanel Success: {panelName}");
        }

        if (useFade)
            panel.HideMe(DoDestroy);
        else
            DoDestroy();

        if (panel.Layer == UILayer.Popup)
            RemovePopupFromStack(panel);

    }

    public T GetPanel<T>() where T : BasePanel
    {
        string panelName = typeof(T).Name;

        if (panelDic.TryGetValue(panelName, out BasePanel panel))
            return panel as T;

        return null;
    }

    public BasePanel GetPanel(string panelName)
    {
        if (panelDic.TryGetValue(panelName, out BasePanel panel))
            return panel;

        return null;
    }

    public void RefreshPanel<T>() where T : BasePanel
    {
        RefreshPanel(typeof(T).Name);
    }

    public void RefreshPanel(string panelName)
    {
        if (panelDic.TryGetValue(panelName, out BasePanel panel))
        {
            panel.Refresh();
        }
        else
        {
            Debug.LogWarning($"[UIManager] RefreshPanel failed, panel not found: {panelName}");
        }
    }

    public bool IsPanelVisible<T>() where T : BasePanel
    {
        return IsPanelVisible(typeof(T).Name);
    }

    public bool IsPanelVisible(string panelName)
    {
        if (panelDic.TryGetValue(panelName, out BasePanel panel))
            return panel.IsVisible && panel.gameObject.activeSelf;

        return false;
    }


    public void Clear()
    {
        EventBus.Unsubscribe<OpenPanelEvent>(OnOpenPanelEvent);
        EventBus.Unsubscribe<ClosePanelEvent>(OnClosePanelEvent);
        EventBus.Unsubscribe<OpenRoleInfoPanelEvent>(OnOpenRoleInfoPanelEvent);
        EventBus.Unsubscribe<OpenMainPageEvent>(OnOpenMainPageEvent);

        foreach (var kv in panelDic)
        {
            if (kv.Value != null)
            {
                kv.Value.DestroyPanel();
            }
        }

        panelDic.Clear();
        layerDic.Clear();

        if (canvasObj != null)
        {
            GameObject.Destroy(canvasObj);
            canvasObj = null;
        }

        canvasTrans = null;
        isInited = false;

        popupStack.Clear();
        uiMask = null;

        currentMainPage = null;


        Debug.Log("[UIManager] Clear Success.");
    }

    private bool CheckInit()
    {
        if (!isInited || canvasTrans == null)
        {
            Debug.LogError("[UIManager] Not initialized. Please call UIManager.Instance.Init() first.");
            return false;
        }

        return true;
    }
}
