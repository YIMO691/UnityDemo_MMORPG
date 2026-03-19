using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PoolMonitorPanel : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject root;
    [SerializeField] private Transform contentRoot;
    [SerializeField] private PoolMonitorItem itemPrefab;
    [SerializeField] private TMP_Text txtSummary;

    [Header("Refresh")]
    [SerializeField] private bool refreshOnEnable = true;
    [SerializeField] private bool autoRefresh = true;
    [SerializeField] private float refreshInterval = 0.5f;

    [Header("HotKey")]
    [SerializeField] private bool enableHotKey = true;
    [SerializeField] private KeyCode toggleKey = KeyCode.F8;

    private readonly List<PoolMonitorItem> itemList = new List<PoolMonitorItem>();
    private float timer;

    private void Awake()
    {
        if (root == null) root = gameObject;
    }

    private void Start()
    {
        Debug.Log("[PoolMonitorPanel] Start");
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        SetVisible(false);
#else
    gameObject.SetActive(false);
#endif
    }


    private void OnEnable()
    {
        if (refreshOnEnable) RefreshView();
    }

    private void Update()
    {
        if (enableHotKey && Input.GetKeyDown(toggleKey))
        {
            Debug.Log("[PoolMonitorPanel] F8 Pressed");
            SetVisible(!root.activeSelf);
        }

        if (!root.activeSelf)
            return;

        if (!autoRefresh)
            return;

        timer += Time.unscaledDeltaTime;
        if (timer >= refreshInterval)
        {
            timer = 0f;
            RefreshView();
        }
    }


    public void SetVisible(bool visible)
    {
        root.SetActive(visible);
        if (visible)
        {
            timer = 0f;
            RefreshView();
        }
    }

    public void ToggleVisible()
    {
        SetVisible(!root.activeSelf);
    }

    public void RefreshView()
    {
        if (PoolManager.Instance == null) return;
        List<PoolStats> statsList = PoolManager.Instance.GetAllPoolStats();
        EnsureItemCount(statsList.Count);

        int totalPools = statsList.Count;
        int totalObjs = 0, totalActive = 0, totalInactive = 0;

        for (int i = 0; i < itemList.Count; i++)
        {
            bool active = i < statsList.Count;
            itemList[i].gameObject.SetActive(active);
            if (active)
            {
                var s = statsList[i];
                totalObjs += s.Total;
                totalActive += s.Active;
                totalInactive += s.Inactive;
                itemList[i].Bind(s);
            }
        }

        if (txtSummary != null)
            txtSummary.text = $"Pools:{totalPools}  Total:{totalObjs}  Active:{totalActive}  Inactive:{totalInactive}";
    }

    private void EnsureItemCount(int count)
    {
        if (itemPrefab == null || contentRoot == null) return;
        while (itemList.Count < count)
        {
            var item = Instantiate(itemPrefab, contentRoot);
            itemList.Add(item);
        }
    }
}
