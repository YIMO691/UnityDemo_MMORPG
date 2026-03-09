using UnityEngine;
using UnityEngine.UI;

public class BeginPanel : BasePanel
{
    public Button btnStart;
    public Button btnSetting;
    public Button btnQuit;

    public override void Init()
    {
        btnStart.onClick.AddListener(OnClickStart);
        btnSetting.onClick.AddListener(OnClickSetting);
        btnQuit.onClick.AddListener(OnClickQuit);
    }

    private void OnClickStart()
    {
        Debug.Log("Start Game");
    }

    private void OnClickSetting()
    {
        Debug.Log("Open Setting");
    }

    private void OnClickQuit()
    {
        Debug.Log("Quit Game");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
