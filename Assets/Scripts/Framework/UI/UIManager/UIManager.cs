using System.Collections.Generic;
using UnityEngine;

public class UIManager
{
    // 使用单例模式，让 UIManager 在整个游戏中只有一个实例
    private static UIManager instance = new UIManager();
    public static UIManager Instance => instance;

    // 存储所有面板的字典：Key = 面板名字，Value = 面板脚本
    private Dictionary<string, BasePanel> panelDic = new Dictionary<string, BasePanel>();

    // Canvas 对象的 Transform，用作所有面板的父节点
    private Transform canvasTrans;

    // 构造函数私有化
    private UIManager() { }

    /// <summary>
    /// 初始化 UIManager，创建并缓存 PanelCanvas
    /// </summary>
    public void Init()
    {
        if (canvasTrans != null)
            return;

        // 从 ResourceManager 中加载 PanelCanvas 预制体
        GameObject canvasPrefab = ResourceManager.Instance.Load<GameObject>("UI/Root/PanelCanvas");
        if (canvasPrefab == null)
        {
            Debug.LogError("[UIManager] PanelCanvas prefab not found: UI/Windows/PanelCanvas");
            return;
        }

        GameObject canvasObj = GameObject.Instantiate(canvasPrefab);
        canvasTrans = canvasObj.transform;

        GameObject.DontDestroyOnLoad(canvasObj);

        Debug.Log("[UIManager] Init Success.");
    }

    /// <summary>
    /// 显示面板
    /// </summary>
    public T ShowPanel<T>() where T : BasePanel
    {
        if (canvasTrans == null)
        {
            Debug.LogError("[UIManager] Not initialized. Please call UIManager.Instance.Init() first.");
            return null;
        }

        string panelName = typeof(T).Name;

        // 如果已经打开，就直接返回
        if (panelDic.ContainsKey(panelName))
            return panelDic[panelName] as T;

        // 从 ResourceManager 中加载对应面板预制体
        GameObject panelPrefab = ResourceManager.Instance.Load<GameObject>("UI/Windows/" + panelName);
        if (panelPrefab == null)
        {
            Debug.LogError($"[UIManager] Panel prefab not found: UI/Windows/{panelName}");
            return null;
        }

        // 实例化面板并挂到 Canvas 下
        GameObject panelObj = GameObject.Instantiate(panelPrefab);
        panelObj.transform.SetParent(canvasTrans, false);

        // 获取脚本
        T panel = panelObj.GetComponent<T>();
        if (panel == null)
        {
            Debug.LogError($"[UIManager] Component {panelName} not found on prefab.");
            GameObject.Destroy(panelObj);
            return null;
        }

        panelDic.Add(panelName, panel);
        panel.ShowMe();

        Debug.Log($"[UIManager] ShowPanel Success: {panelName}");

        return panel;
    }

    /// <summary>
    /// 隐藏面板
    /// </summary>
    public void HidePanel<T>(bool isFade = true) where T : BasePanel
    {
        string panelName = typeof(T).Name;

        // 如果面板不存在，就直接返回
        if (!panelDic.ContainsKey(panelName))
        {
            Debug.LogWarning($"[UIManager] HidePanel failed, panel not found: {panelName}");
            return;
        }

        BasePanel panel = panelDic[panelName];

        // 如果 isFade 为 true，就先播放隐藏动画，等动画结束后再销毁面板；否则直接销毁面板
        if (isFade)
        {
            panel.HideMe(() =>
            {
                if (panel != null)
                    GameObject.Destroy(panel.gameObject);

                panelDic.Remove(panelName);
                Debug.Log($"[UIManager] HidePanel Success: {panelName}");
            });
        }
        else
        {
            if (panel != null)
                GameObject.Destroy(panel.gameObject);

            panelDic.Remove(panelName);
            Debug.Log($"[UIManager] HidePanel Success: {panelName}");
        }
    }

    /// <summary>
    /// 获取已经显示的面板
    /// </summary>
    public T GetPanel<T>() where T : BasePanel
    {
        string panelName = typeof(T).Name;

        if (panelDic.ContainsKey(panelName))
            return panelDic[panelName] as T;

        return null;
    }
}
