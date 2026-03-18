using UnityEngine;
using Game;

[RequireComponent(typeof(StarterAssetsInputs))]
public class PlayerNavigator : BaseNavigator
{
    [SerializeField] private bool autoSprint = true;
    [SerializeField] private float manualCancelThreshold = 0.2f;

    private StarterAssetsInputs input;
    private Camera mainCamera;

    protected override void Awake()
    {
        base.Awake();
        input = GetComponent<StarterAssetsInputs>();
    }

    private void Start()
    {
        mainCamera = Camera.main;
    }

    protected override void Update()
    {
        if (mainCamera == null) mainCamera = Camera.main;
        if (IsNavigating && HasManualInput())
        {
            StopNavigation();
            return;
        }
        base.Update();
    }

    protected override void TickNavigation()
    {
        Vector3 target = pathPoints[currentIndex];
        Vector3 currentPos = transform.position;
        Vector3 flatTarget = new Vector3(target.x, currentPos.y, target.z);
        Vector3 toTarget = flatTarget - currentPos;
        float distance = toTarget.magnitude;
        AdvanceIfReached(distance);
        if (!IsNavigating) return;
        Vector3 worldDir = toTarget.normalized;
        Vector2 moveInput = ConvertWorldDirectionToCameraInput(worldDir);
        input.move = moveInput;
        input.sprint = autoSprint;
        input.jump = false;
    }

    protected override void OnNavigationStopped()
    {
        if (input == null) return;
        input.move = Vector2.zero;
        input.sprint = false;
        input.jump = false;
    }

    private bool HasManualInput()
    {
        if (input == null) return false;
        return input.move.sqrMagnitude > manualCancelThreshold * manualCancelThreshold
               || input.jump
               || input.look.sqrMagnitude > 0.001f;
    }

    private Vector2 ConvertWorldDirectionToCameraInput(Vector3 worldDir)
    {
        if (mainCamera == null) return new Vector2(worldDir.x, worldDir.z);
        Vector3 camForward = mainCamera.transform.forward;
        Vector3 camRight = mainCamera.transform.right;
        camForward.y = 0f;
        camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();
        float x = Vector3.Dot(worldDir, camRight);
        float y = Vector3.Dot(worldDir, camForward);
        Vector2 result = new Vector2(x, y);
        if (result.sqrMagnitude > 1f) result.Normalize();
        return result;
    }
}
