using UnityEngine.SceneManagement;

public static class SceneNavigator
{
    public static void EnterBeginScene()
    {
        SceneManager.LoadScene(SceneNames.BeginScene);
    }

    public static void EnterGameScene()
    {
        SceneManager.LoadScene(SceneNames.GameScene);
    }
}

