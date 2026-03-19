using System.Collections.Generic;
using UnityEngine;

public class PoolManager : MonoBehaviour
{
    private static PoolManager _instance;
    public static PoolManager Instance
    {
        get
        {
            if (_instance == null)
            {
                var go = new GameObject("[PoolManager]");
                _instance = go.AddComponent<PoolManager>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }

    private readonly Dictionary<string, ObjectPool> pools = new Dictionary<string, ObjectPool>();
    private Transform poolRoot;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);

        poolRoot = new GameObject("[PoolRoot]").transform;
        DontDestroyOnLoad(poolRoot.gameObject);
    }

    public void RegisterPool(string key, GameObject prefab, int preloadCount = 0)
    {
        if (string.IsNullOrEmpty(key) || prefab == null)
        {
            Debug.LogError("[PoolManager] RegisterPool 参数非法");
            return;
        }
        if (pools.ContainsKey(key)) return;
        Transform root = new GameObject($"Pool_{key}").transform;
        root.SetParent(poolRoot, false);
        pools[key] = new ObjectPool(key, prefab, root, preloadCount);
    }

    public bool Contains(string key) => pools.ContainsKey(key);

    public GameObject Spawn(string key, Transform parent = null)
    {
        if (!pools.TryGetValue(key, out var pool))
        {
            Debug.LogError("[PoolManager] 未注册对象池: " + key);
            return null;
        }
        return pool.Spawn(parent);
    }

    public bool TryGetPool(string key, out ObjectPool pool)
    {
        return pools.TryGetValue(key, out pool);
    }

    public bool TryGetPoolStats(string key, out PoolStats stats)
    {
        stats = default;
        if (!pools.TryGetValue(key, out var pool)) return false;
        stats = pool.GetStats();
        return true;
    }

    public List<PoolStats> GetAllPoolStats()
    {
        var result = new List<PoolStats>();
        foreach (var kv in pools)
        {
            result.Add(kv.Value.GetStats());
        }
        result.Sort((a, b) => string.CompareOrdinal(a.PoolKey, b.PoolKey));
        return result;
    }

    public string GetPoolDebugInfo(string key)
    {
        if (!pools.TryGetValue(key, out var pool))
            return $"Pool[{key}] not found";
        return $"Pool[{key}] Total={pool.TotalCount}, Active={pool.ActiveCount}, Inactive={pool.InactiveCount}, Create={pool.CreateCount}, Spawn={pool.SpawnCount}, Recycle={pool.RecycleCount}";
    }

    public void Recycle(GameObject go)
    {
        if (go == null) return;
        var item = go.GetComponent<PoolItem>();
        if (item == null || string.IsNullOrEmpty(item.PoolKey))
        {
            Debug.LogWarning("[PoolManager] 回收对象缺少 PoolItem，直接销毁: " + go.name);
            Destroy(go);
            return;
        }
        if (!pools.TryGetValue(item.PoolKey, out var pool))
        {
            Debug.LogWarning("[PoolManager] 找不到对应池，直接销毁: " + item.PoolKey);
            Destroy(go);
            return;
        }
        pool.Recycle(go);
    }
}
