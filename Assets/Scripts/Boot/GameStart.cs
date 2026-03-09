using UnityEngine;

public class GameStart : MonoBehaviour
{
    private void Start()
    {
        UIManager.Instance.Init();
        UIManager.Instance.ShowPanel<BeginPanel>();
    }
}
