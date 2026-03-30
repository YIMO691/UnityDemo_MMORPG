using System.IO;
using UnityEditor;
using UnityEngine;

public static class AssetBundleBuilderEditor
{
    private const string OutputRoot = "AssetBundleOutput";
    private const string MenuRoot = "Tools/AssetBundle/";

    [MenuItem(MenuRoot + "3. 打包 Windows")]
    public static void BuildWindows()
    {
        Build(BuildTarget.StandaloneWindows64);
    }

    [MenuItem(MenuRoot + "4. 打包 Android")]
    public static void BuildAndroid()
    {
        Build(BuildTarget.Android);
    }

    [MenuItem(MenuRoot + "5. 打包 iOS")]
    public static void BuildIOS()
    {
        Build(BuildTarget.iOS);
    }

    private static void Build(BuildTarget target)
    {
        string outDir = Path.Combine(OutputRoot, target.ToString());
        if (!Directory.Exists(outDir))
            Directory.CreateDirectory(outDir);

        BuildAssetBundleOptions options =
            BuildAssetBundleOptions.ChunkBasedCompression |
            BuildAssetBundleOptions.StrictMode;

        BuildPipeline.BuildAssetBundles(outDir, options, target);
        AssetDatabase.Refresh();

        Debug.Log("AssetBundle 打包完成: " + Path.GetFullPath(outDir));
    }
}
