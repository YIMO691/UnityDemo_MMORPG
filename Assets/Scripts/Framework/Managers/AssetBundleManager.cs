using System.Collections.Generic;
using System.IO;
using UnityEngine;

public sealed class AssetBundleManager
{
    private static AssetBundleManager _instance;
    public static AssetBundleManager Instance => _instance ?? (_instance = new AssetBundleManager());

    private readonly Dictionary<string, AssetBundle> _loadedBundles = new Dictionary<string, AssetBundle>();
    private AssetBundleManifest _manifest;
    private string _bundleRoot;

    private AssetBundleManager() { }

    public void Initialize(string bundleRoot = null)
    {
        _bundleRoot = bundleRoot;
        if (string.IsNullOrEmpty(_bundleRoot))
        {
            _bundleRoot = Path.Combine(Application.streamingAssetsPath, GetPlatformFolderName());
        }

        string manifestBundlePath = Path.Combine(_bundleRoot, GetPlatformFolderName());
        if (!File.Exists(manifestBundlePath))
        {
            Debug.LogWarning("未找到主 manifest bundle: " + manifestBundlePath);
            return;
        }

        AssetBundle manifestBundle = AssetBundle.LoadFromFile(manifestBundlePath);
        if (manifestBundle == null)
        {
            Debug.LogError("加载 manifest 失败: " + manifestBundlePath);
            return;
        }

        _manifest = manifestBundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
        if (_manifest == null)
        {
            Debug.LogError("读取 AssetBundleManifest 失败。");
        }
    }

    public T LoadAsset<T>(string bundleName, string assetName) where T : UnityEngine.Object
    {
        AssetBundle bundle = LoadBundleWithDependencies(bundleName);
        if (bundle == null)
            return null;
        return bundle.LoadAsset<T>(assetName);
    }

    public T LoadByResourcesStylePath<T>(string resourceStylePath) where T : UnityEngine.Object
    {
        string bundleName = ConvertResourcePathToBundleName(resourceStylePath);
        string assetName = Path.GetFileName(resourceStylePath);
        AssetBundle bundle = LoadBundleWithDependencies(bundleName);
        if (bundle != null)
        {
            T asset = bundle.LoadAsset<T>(assetName);
            if (asset != null)
                return asset;
        }
        return null;
    }

    public void UnloadBundle(string bundleName, bool unloadAllLoadedObjects = false)
    {
        if (_loadedBundles.TryGetValue(bundleName, out AssetBundle bundle))
        {
            bundle.Unload(unloadAllLoadedObjects);
            _loadedBundles.Remove(bundleName);
        }
    }

    public void UnloadAllBundles(bool unloadAllLoadedObjects = false)
    {
        foreach (var kv in _loadedBundles)
        {
            kv.Value.Unload(unloadAllLoadedObjects);
        }
        _loadedBundles.Clear();
    }

    private AssetBundle LoadBundleWithDependencies(string bundleName)
    {
        if (string.IsNullOrEmpty(bundleName))
            return null;
        if (_loadedBundles.TryGetValue(bundleName, out AssetBundle loaded))
            return loaded;

        if (_manifest != null)
        {
            string[] deps = _manifest.GetAllDependencies(bundleName);
            for (int i = 0; i < deps.Length; i++)
            {
                LoadBundleWithDependencies(deps[i]);
            }
        }

        string fullPath = Path.Combine(_bundleRoot, bundleName);
        if (!File.Exists(fullPath))
        {
            return null;
        }

        AssetBundle bundle = AssetBundle.LoadFromFile(fullPath);
        if (bundle == null)
        {
            Debug.LogError("Bundle 加载失败: " + fullPath);
            return null;
        }

        _loadedBundles[bundleName] = bundle;
        return bundle;
    }

    private string ConvertResourcePathToBundleName(string resourceStylePath)
    {
        if (string.IsNullOrWhiteSpace(resourceStylePath))
            return null;
        string normalized = resourceStylePath.Replace("\\", "/").Trim('/');
        int lastSlash = normalized.LastIndexOf('/');
        if (lastSlash < 0)
            return "ab.root";
        string dir = normalized.Substring(0, lastSlash);
        return "ab." + dir.Replace("/", ".").ToLowerInvariant();
    }

    private string GetPlatformFolderName()
    {
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
        return "StandaloneWindows64";
#elif UNITY_ANDROID
        return "Android";
#elif UNITY_IOS
        return "iOS";
#elif UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
        return "StandaloneOSX";
#else
        return Application.platform.ToString();
#endif
    }
}
