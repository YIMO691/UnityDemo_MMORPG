using UnityEngine;

public class PlayerProgressionDebugController : MonoBehaviour
{
    [SerializeField] private int smallExp = 30;
    [SerializeField] private int bigExp = 120;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F6))
        {
            PlayerProgressionService.Instance.AddExpToCurrentPlayer(smallExp);
        }

        if (Input.GetKeyDown(KeyCode.F7))
        {
            PlayerProgressionService.Instance.AddExpToCurrentPlayer(bigExp);
        }
    }
}
