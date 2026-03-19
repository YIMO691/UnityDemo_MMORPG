using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;

public static class SceneMenu
{
    [MenuItem("Tools/Scenes/Open BeginScene")]
    public static void OpenBeginScene()
    {
        if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            EditorSceneManager.OpenScene($"Assets/Scenes/{SceneNames.BeginScene}.unity", OpenSceneMode.Single);
        }
    }

    [MenuItem("Tools/Scenes/Open GameScene")]
    public static void OpenGameScene()
    {
        if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            EditorSceneManager.OpenScene($"Assets/Scenes/{SceneNames.GameScene}.unity", OpenSceneMode.Single);
        }
    }

    [MenuItem("Tools/Save/Save Current Player Transform")]
    public static void SaveCurrentPlayerTransform()
    {
        var entry = Object.FindObjectOfType<GameSceneEntry>();
        if (entry == null)
        {
            Debug.LogWarning("[SceneMenu] GameSceneEntry not found in current scene.");
            return;
        }

        entry.SaveCurrentPlayerTransform();
        Debug.Log("[SceneMenu] SaveCurrentPlayerTransform executed.");
    }
}
