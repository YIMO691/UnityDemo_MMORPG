using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(StarterAssetsInputs))]
public class PlayerNavigator : BaseNavigator
{
    [SerializeField] private bool autoSprint = true;
    [SerializeField] private float manualCancelThreshold = 0.2f;
    [SerializeField] private float cancelGraceTime = 0.8f;

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
        //玩家还没注册就开始点击
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
                    // 让服务端统一清理可视化状态与路径
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
            // 不再用鼠标 delta 作为取消条件；左键点击地图也不取消
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

    // 保留无参版本给潜在调用
    private bool HasManualInput()
    {
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
            if (Mouse.current.rightButton.isPressed) return true;
        }

        if (Gamepad.current != null)
        {
            if (Gamepad.current.leftStick.ReadValue().sqrMagnitude > 0.01f) return true;
            if (Gamepad.current.rightStick.ReadValue().sqrMagnitude > 0.01f) return true;
            if (Gamepad.current.buttonSouth.isPressed) return true;
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
        if (result.sqrMagnitude > 0.0001f) result = result.normalized;
        else result = Vector2.zero;
        return result;
    }
}
