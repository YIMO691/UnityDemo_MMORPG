using UnityEngine;

public class AssetBundleLoader : IResLoader
{
    public T Load<T>(string path) where T : Object
    {
#if ENABLE_ASSETBUNDLE_RUNTIME
        var ab = AssetBundleManager.Instance.LoadByResourcesStylePath<T>(path);
        if (ab != null) return ab;
#endif
        return Resources.Load<T>(path);
    }
}
