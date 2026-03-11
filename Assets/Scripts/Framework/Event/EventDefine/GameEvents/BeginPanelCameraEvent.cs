using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CameraEvent : MonoBehaviour
{
    private Animator animator;

    //使用 UnityAction 来存储回调函数，方便在动画结束时调用
    private UnityAction overAction;
    // Start is called before the first frame update
    void Start()
    {
        animator = this.GetComponent<Animator>();
    }

    public void TurnAround(UnityAction action)
    {
        animator.SetTrigger("Turn");
        overAction = action;
    }

    public void PlayerOver()
    {
        overAction?.Invoke();
        overAction = null;
    }
}
