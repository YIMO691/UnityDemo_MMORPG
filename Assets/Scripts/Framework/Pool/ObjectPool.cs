using System.Collections.Generic;
using UnityEngine;

public class ObjectPool
{
    public string Key { get; private set; }
    public GameObject Prefab { get; private set; }
    public Transform Root { get; private set; }

    private readonly Queue<GameObject> queue = new Queue<GameObject>();

    public ObjectPool(string key, GameObject prefab, Transform root, int preloadCount)
    {
        Key = key;
        Prefab = prefab;
        Root = root;

        for (int i = 0; i < preloadCount; i++)
        {
            GameObject go = CreateInstance();
            Recycle(go);
        }
    }

    public GameObject Spawn(Transform parent = null)
    {
        GameObject go = queue.Count > 0 ? queue.Dequeue() : CreateInstance();

        if (parent != null) go.transform.SetParent(parent, false);
        else go.transform.SetParent(null);

        go.SetActive(true);

        var list = go.GetComponentsInChildren<IPoolable>(true);
        for (int i = 0; i < list.Length; i++) list[i].OnSpawnFromPool();

        return go;
    }

    public void Recycle(GameObject go)
    {
        if (go == null) return;
        var list = go.GetComponentsInChildren<IPoolable>(true);
        for (int i = 0; i < list.Length; i++) list[i].OnRecycleToPool();

        go.SetActive(false);
        go.transform.SetParent(Root, false);
        queue.Enqueue(go);
    }

    private GameObject CreateInstance()
    {
        GameObject go = Object.Instantiate(Prefab, Root);
        go.SetActive(false);
        var item = go.GetComponent<PoolItem>();
        if (item == null) item = go.AddComponent<PoolItem>();
        item.PoolKey = Key;
        return go;
    }
}
