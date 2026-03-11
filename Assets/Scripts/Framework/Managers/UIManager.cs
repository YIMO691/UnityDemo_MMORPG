using System.Collections.Generic;
using UnityEngine;

public class UIManager
{
    private Dictionary<UILayer, Transform> layerDic = new Dictionary<UILayer, Transform>();

    private static UIManager instance = new UIManager();
    public static UIManager Instance => instance;

    private readonly Dictionary<string, BasePanel> panelDic = new Dictionary<string, BasePanel>();
    private Transform canvasTrans;

    private UIManager() { }

    void InitLayer()
    {
        if (canvasTrans == null)
        {
            Debug.LogError("[UIManager] canvasTrans is null when InitLayer()");
            return;
        }

        layerDic.Clear();

        foreach (UILayer layer in System.Enum.GetValues(typeof(UILayer)))
        {
            Transform layerTrans = canvasTrans.Find(layer.ToString());

            if (layerTrans == null)
            {
                Debug.LogError($"[UIManager] Layer not found: {layer}");
                continue;
            }

            layerDic.Add(layer, layerTrans);
        }
    }


    public void Init()
    {
        // 如果已经初始化过了，就直接返回，避免重复初始化
        if (canvasTrans != null)
            return;

        EventBus.Subscribe<OpenPanelEvent>(OnOpenPanelEvent);
        EventBus.Subscribe<ClosePanelEvent>(OnClosePanelEvent);

        GameObject canvasPrefab = ResourceManager.Instance.Load<GameObject>("UI/Root/PanelCanvas");
        if (canvasPrefab == null)
        {
            Debug.LogError("[UIManager] PanelCanvas prefab not found: UI/Root/PanelCanvas");
            return;
        }

        GameObject canvasObj = GameObject.Instantiate(canvasPrefab);
        canvasTrans = canvasObj.transform;
        GameObject.DontDestroyOnLoad(canvasObj);

        // 对于层级的初始化，必须在创建 Canvas 之后进行，否则会找不到层级对象
        InitLayer();
        Debug.Log("[UIManager] Init Success.");
    }

    private void OnOpenPanelEvent(OpenPanelEvent e)
    {
        ShowPanel(e.PanelName);
    }

    private void OnClosePanelEvent(ClosePanelEvent e)
    {
        HidePanel(e.PanelName);
    }

    public T ShowPanel<T>() where T : BasePanel
    {
        return ShowPanel(typeof(T).Name) as T;
    }

    public BasePanel ShowPanel(string panelName)
    {
        if (canvasTrans == null)
        {
            Debug.LogError("[UIManager] Not initialized. Please call UIManager.Instance.Init() first.");
            return null;
        }

        if (panelDic.ContainsKey(panelName))
            return panelDic[panelName];

        GameObject panelPrefab = ResourceManager.Instance.Load<GameObject>("UI/Windows/" + panelName);
        if (panelPrefab == null)
        {
            Debug.LogError($"[UIManager] Panel prefab not found: UI/Windows/{panelName}");
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

        if (!layerDic.ContainsKey(layer))
        {
            Debug.LogError($"Layer not exist: {layer}");
            return null;
        }

        panelObj.transform.SetParent(layerDic[layer], false);
        panelDic.Add(panelName, panel);
        panel.ShowMe();

        Debug.Log($"[UIManager] ShowPanel Success: {panelName}");
        return panel;
    }

    public void HidePanel<T>(bool isFade = true) where T : BasePanel
    {
        HidePanel(typeof(T).Name, isFade);
    }

    public void HidePanel(string panelName, bool isFade = true)
    {
        if (!panelDic.ContainsKey(panelName))
        {
            Debug.LogWarning($"[UIManager] HidePanel failed, panel not found: {panelName}");
            return;
        }

        BasePanel panel = panelDic[panelName];

        void DestroyPanel()
        {
            if (panel != null)
                GameObject.Destroy(panel.gameObject);

            panelDic.Remove(panelName);
            Debug.Log($"[UIManager] HidePanel Success: {panelName}");
        }

        if (isFade)
            panel.HideMe(DestroyPanel);
        else
            DestroyPanel();
    }

    public T GetPanel<T>() where T : BasePanel
    {
        string panelName = typeof(T).Name;

        if (panelDic.ContainsKey(panelName))
            return panelDic[panelName] as T;

        return null;
    }

    public void Clear()
    {
        EventBus.Unsubscribe<OpenPanelEvent>(OnOpenPanelEvent);
        EventBus.Unsubscribe<ClosePanelEvent>(OnClosePanelEvent);
        panelDic.Clear();
        canvasTrans = null;
    }

}
