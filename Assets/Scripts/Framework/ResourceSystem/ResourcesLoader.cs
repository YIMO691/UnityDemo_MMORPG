using UnityEngine;

//使用Resources加载资源的类 实现IResLoader接口
public class ResourcesLoader : IResLoader
{
    // 加载资源的方法，传入资源路径，返回资源对象
    public T Load<T>(string path) where T : Object
    {
        T res = Resources.Load<T>(path);
        if (res == null)
        {
            Debug.LogError($"[ResourcesLoader] Load Failed: {path}");
        }
        else
        {
            Debug.Log($"[ResourcesLoader] Load Success: {path}");
        }

        return res;
    }
}
