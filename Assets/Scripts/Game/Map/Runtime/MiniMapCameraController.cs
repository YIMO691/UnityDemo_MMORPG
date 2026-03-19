using UnityEngine;

public class MiniMapCameraController : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float height = 25f;
    [SerializeField] private Vector3 offset = Vector3.zero;
    [SerializeField] private bool followRotation = false;

    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 pos = target.position + offset;
        pos.y = target.position.y + height;
        transform.position = pos;

        if (followRotation)
        {
            transform.rotation = Quaternion.Euler(90f, target.eulerAngles.y, 0f);
        }
        else
        {
            transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        }
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        Debug.Log("[MiniMapCameraController] SetTarget -> " + (newTarget != null ? newTarget.name : "null"));
    }
}
