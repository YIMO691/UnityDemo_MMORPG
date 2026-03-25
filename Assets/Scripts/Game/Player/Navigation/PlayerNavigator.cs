using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInputProxy))]
public class PlayerNavigator : BaseNavigator
{
    [SerializeField] private bool autoSprint = true;
    [SerializeField] private float manualCancelThreshold = 0.2f;
    [SerializeField] private float cancelGraceTime = 0.8f;

    private PlayerInputProxy inputProxy;
    private Camera mainCamera;
    private float lastSetPathTime;

    protected override void Awake()
    {
        base.Awake();
        inputProxy = GetComponent<PlayerInputProxy>();
    }

    private void Start()
    {
        mainCamera = Camera.main;
        NavigationRegistry.Instance.Register(this);
    }

    public override void SetPath(Vector3[] newPath, float newStopDistance)
    {
        base.SetPath(newPath, newStopDistance);
        lastSetPathTime = Time.time;

        if (newPath != null)
        {
            Debug.Log("[PlayerNavigator] SetPath，角点数 = " + newPath.Length);
        }
    }

    protected override void Update()
    {
        if (mainCamera == null) mainCamera = Camera.main;

        if (IsNavigating)
        {
            bool inGrace = Time.time - lastSetPathTime < cancelGraceTime;
            if (!inGrace)
            {
                string reason;
                if (HasManualInput(out reason))
                {
                    Debug.Log("[PlayerNavigator] 检测到手动输入，取消自动寻路。原因: " + reason);
                    EventBus.Publish(new NavigationStopRequestEvent(NavigationConsts.PlayerAgentId));
                    return;
                }
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
        if (!IsNavigating)
        {
            inputProxy?.ClearNavigationCommand();
            return;
        }

        Vector3 worldDir = toTarget.normalized;
        Vector2 moveInput = ConvertWorldDirectionToCameraInput(worldDir);

        var cmd = new PlayerControlCommand
        {
            move = moveInput,
            sprint = autoSprint,
            jump = false,
            look = Vector2.zero
        };

        inputProxy?.SetNavigationCommand(cmd);
    }

    protected override void OnNavigationStopped()
    {
        inputProxy?.ClearNavigationCommand();
    }

    private bool HasManualInput(out string reason)
    {
        reason = null;

#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null)
        {
            if (Keyboard.current.wKey.isPressed) { reason = "W"; return true; }
            if (Keyboard.current.aKey.isPressed) { reason = "A"; return true; }
            if (Keyboard.current.sKey.isPressed) { reason = "S"; return true; }
            if (Keyboard.current.dKey.isPressed) { reason = "D"; return true; }
            if (Keyboard.current.spaceKey.isPressed) { reason = "Space"; return true; }
        }

        if (Mouse.current != null)
        {
            if (Mouse.current.rightButton.isPressed) { reason = "MouseRight"; return true; }
        }

        if (Gamepad.current != null)
        {
            if (Gamepad.current.leftStick.ReadValue().sqrMagnitude > 0.01f) { reason = "GamepadLeftStick"; return true; }
            if (Gamepad.current.rightStick.ReadValue().sqrMagnitude > 0.01f) { reason = "GamepadRightStick"; return true; }
            if (Gamepad.current.buttonSouth.isPressed) { reason = "GamepadSouth"; return true; }
        }
#endif
        return false;
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
        return result.sqrMagnitude > 0.0001f ? result.normalized : Vector2.zero;
    }
}
