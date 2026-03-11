using UnityEngine;

// 使用AssetBundle加载资源的类 实现IResLoader接口
public class AssetBundleLoader : IResLoader
{
    public T Load<T>(string path) where T : Object
    {
        Debug.LogWarning($"[AssetBundleLoader] Not Implemented Yet. Path: {path}");
        return null;
    }
}
