using System;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class AssetBundleNameByDirectoryEditor
{
    private const string ResourcesRoot = "Assets/Resources";
    private const string MenuRoot = "Tools/AssetBundle/";

    [MenuItem(MenuRoot + "1. 刷新 AssetBundleName")]
    public static void RefreshAssetBundleNames()
    {
        if (!Directory.Exists(ResourcesRoot))
        {
            Debug.LogError("资源目录不存在: " + ResourcesRoot);
            return;
        }

        string[] assetPaths = AssetDatabase.GetAllAssetPaths();
        int count = 0;

        try
        {
            AssetDatabase.StartAssetEditing();
            foreach (string assetPath in assetPaths)
            {
                if (!assetPath.StartsWith(ResourcesRoot, StringComparison.OrdinalIgnoreCase))
                    continue;
                if (Directory.Exists(assetPath))
                    continue;
                if (IsMetaOrUnsupported(assetPath))
                    continue;

                string bundleName = BuildBundleName(assetPath);
                AssetImporter importer = AssetImporter.GetAtPath(assetPath);
                if (importer == null)
                    continue;

                if (importer.assetBundleName != bundleName)
                {
                    importer.assetBundleName = bundleName;
                    importer.assetBundleVariant = string.Empty;
                    count++;
                }
            }
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
            AssetDatabase.RemoveUnusedAssetBundleNames();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        Debug.Log("AssetBundleName 刷新完成，共更新 " + count + " 个资源。");
    }

    [MenuItem(MenuRoot + "2. 清空所有 AssetBundleName")]
    public static void ClearAllAssetBundleNames()
    {
        string[] assetPaths = AssetDatabase.GetAllAssetPaths();
        int count = 0;

        try
        {
            AssetDatabase.StartAssetEditing();
            foreach (string assetPath in assetPaths)
            {
                AssetImporter importer = AssetImporter.GetAtPath(assetPath);
                if (importer == null)
                    continue;
                if (!string.IsNullOrEmpty(importer.assetBundleName))
                {
                    importer.assetBundleName = string.Empty;
                    importer.assetBundleVariant = string.Empty;
                    count++;
                }
            }
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
            AssetDatabase.RemoveUnusedAssetBundleNames();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        Debug.Log("已清空 " + count + " 个资源的 AssetBundleName。");
    }

    private static string BuildBundleName(string assetPath)
    {
        string relative = assetPath.Substring(ResourcesRoot.Length).TrimStart('/', '\\');
        string directory = Path.GetDirectoryName(relative)?.Replace("\\", "/") ?? string.Empty;
        if (string.IsNullOrWhiteSpace(directory))
            return "ab.root";
        return "ab." + directory.Replace("/", ".").ToLowerInvariant();
    }

    private static bool IsMetaOrUnsupported(string assetPath)
    {
        string ext = Path.GetExtension(assetPath).ToLowerInvariant();
        if (ext == ".meta" || ext == ".cs" || ext == ".js" || ext == ".dll")
            return true;
        return false;
    }
}
