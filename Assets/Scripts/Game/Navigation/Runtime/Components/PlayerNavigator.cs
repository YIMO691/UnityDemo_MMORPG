using UnityEngine;
using Game;
using UnityEngine.InputSystem;

[RequireComponent(typeof(StarterAssetsInputs))]
public class PlayerNavigator : BaseNavigator
{
    [SerializeField] private bool autoSprint = true;
    [SerializeField] private float manualCancelThreshold = 0.2f;
    [SerializeField] private float cancelGraceTime = 0.25f;

    private StarterAssetsInputs input;
    private Camera mainCamera;
    private float lastSetPathTime;

    protected override void Awake()
    {
        base.Awake();
        input = GetComponent<StarterAssetsInputs>();
    }

    private void Start()
    {
        mainCamera = Camera.main;
    }

    public override void SetPath(Vector3[] newPath, float newStopDistance)
    {
        base.SetPath(newPath, newStopDistance);
        lastSetPathTime = Time.time;
    }

    protected override void Update()
    {
        if (mainCamera == null) mainCamera = Camera.main;
        if (IsNavigating)
        {
            bool inGrace = Time.time - lastSetPathTime < cancelGraceTime;
            if (!inGrace && HasManualInput())
            {
                StopNavigation();
                return;
            }
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

#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null)
        {
            if (Keyboard.current.wKey.isPressed ||
                Keyboard.current.aKey.isPressed ||
                Keyboard.current.sKey.isPressed ||
                Keyboard.current.dKey.isPressed ||
                Keyboard.current.spaceKey.isPressed)
            {
                return true;
            }
        }

        if (Mouse.current != null)
        {
            if (Mouse.current.delta.ReadValue().sqrMagnitude > 0.01f)
                return true;
            if (Mouse.current.rightButton.isPressed)
                return true;
        }

        if (Gamepad.current != null)
        {
            if (Gamepad.current.leftStick.ReadValue().sqrMagnitude > 0.01f) return true;
            if (Gamepad.current.rightStick.ReadValue().sqrMagnitude > 0.01f) return true;
            if (Gamepad.current.buttonSouth.isPressed) return true;
        }
#endif
        return input.move.sqrMagnitude > manualCancelThreshold * manualCancelThreshold
               || input.jump;
    }

    private Vector2 ConvertWorldDirectionToCameraInput(Vector3 worldDir)
    {
        if (mainCamera == null)
        {
            Vector2 v = new Vector2(worldDir.x, worldDir.z);
            return v.sqrMagnitude > 0.0001f ? v.normalized : Vector2.zero;
        }
        Vector3 camForward = mainCamera.transform.forward;
        Vector3 camRight = mainCamera.transform.right;
        camForward.y = 0f;
        camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();
        float x = Vector3.Dot(worldDir, camRight);
        float y = Vector3.Dot(worldDir, camForward);
        Vector2 result = new Vector2(x, y);
        if (result.sqrMagnitude > 0.0001f) result = result.normalized;
        else result = Vector2.zero;
        return result;
    }
}
