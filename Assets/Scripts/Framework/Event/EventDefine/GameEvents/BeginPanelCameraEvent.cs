using UnityEngine;
using UnityEngine.Events;

public class CameraEvent : MonoBehaviour
{
    private Animator animator;
    private UnityAction overAction;

    private Vector3 initPos;
    private Quaternion initRot;

    void Start()
    {
        animator = this.GetComponent<Animator>();
    }

    public void TurnAround(UnityAction action)
    {
        Debug.Log("[CameraEvent] TurnAround() called");

        overAction = action;

        if (animator != null)
        {
            animator.SetTrigger("Turn");
            StartCoroutine(DebugAnimatorState());
        }
        else
        {
            Debug.LogWarning("[CameraEvent] Animator is null");
            overAction?.Invoke();
            overAction = null;
        }
    }

    private System.Collections.IEnumerator DebugAnimatorState()
    {
        yield return null;

        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        Debug.Log($"[CameraEvent] Current State Hash = {stateInfo.shortNameHash}, normalizedTime = {stateInfo.normalizedTime}");
    }

    public void PlayerOver()
    {
        Debug.Log("[CameraEvent] PlayerOver() Animation Event triggered");

        overAction?.Invoke();
        overAction = null;
    }

    /// <summary>
    /// 重置摄像头到初始状态
    /// </summary>
    public void ResetCamera()
    {
        if (animator != null)
        {
            animator.Rebind();
            animator.Update(0);
        }

        transform.position = initPos;
        transform.rotation = initRot;
    }
}
