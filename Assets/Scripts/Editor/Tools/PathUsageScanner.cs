using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

public static class PathUsageScanner
{
    [MenuItem("Tools/Path Scanner/Scan Hardcoded Paths")]
    public static void Scan()
    {
        int issues = 0;
        var root = Application.dataPath + "/Scripts";
        var files = Directory.GetFiles(root, "*.cs", SearchOption.AllDirectories);
        var hardPath = new Regex("\"(UI/|Role/|Map/|Config/)[^\"]+\"");
        var loadLiteral = new Regex("(Resources\\.Load|ResourceManager\\.Instance\\.Load)\\s*<[^>]+>\\s*\\(\\s*\\\"[^\\\"]+\\\"\\s*\\)");
        foreach (var f in files)
        {
            if (f.EndsWith("AssetPaths.cs")) continue;
            var text = File.ReadAllText(f);
            if (hardPath.IsMatch(text) || loadLiteral.IsMatch(text))
            {
                issues++;
                Debug.LogWarning("Hardcoded path candidate: " + f);
            }
        }
        if (issues == 0) Debug.Log("Path scan complete. No hardcoded paths found.");
        else Debug.LogWarning("Path scan complete. Issues: " + issues);
    }
}
