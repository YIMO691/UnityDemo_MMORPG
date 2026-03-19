using UnityEngine;

public class ResourceManager
{
    // 单例模式实现资源管理器
    private static ResourceManager instance = new ResourceManager();
    public static ResourceManager Instance => instance;

    // 资源加载器接口 根据不同的加载模式使用不同的加载器
    private IResLoader loader;
    private LoadMode currentMode;

    // 构造函数私有化 让外部无法创建ResourceManager实例
    private ResourceManager()
    {
        // 在构造函数中根据平台设置默认的加载模式 编辑器使用Resources 其他平台使用AssetBundle
#if UNITY_EDITOR
        SetLoadMode(LoadMode.Resources);
#else
        SetLoadMode(LoadMode.AssetBundle);
#endif
    }

    // 设置加载模式 根据传入的模式创建对应的加载器
    public void SetLoadMode(LoadMode mode)
    {
        currentMode = mode;

        switch (currentMode)
        {
            case LoadMode.Resources:
                loader = new ResourcesLoader();
                break;
            case LoadMode.AssetBundle:
                loader = new AssetBundleLoader();
                break;
        }

        Debug.Log($"[ResourceManager] Current Load Mode: {currentMode}");
    }

    // 加载资源的公共接口 根据当前的加载器进行资源加载
    public T Load<T>(string path) where T : Object
    {
        if (loader == null)
        {
            Debug.LogError("[ResourceManager] loader is null.");
            return null;
        }

        return loader.Load<T>(path);
    }

    // 获取当前的加载模式
    public LoadMode GetLoadMode()
    {
        return currentMode;
    }
}

//总结：
// ResourceManager类是一个单例类 用于管理游戏中的资源加载 根据不同的平台和需求 可以切换不同的加载模式 目前支持Resources和AssetBundle两种加载方式 通过实现IResLoader接口 来统一资源加载的接口 方便后续扩展其他的加载方式