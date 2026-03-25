using UnityEngine;

[RequireComponent(typeof(PlayerInputProxy))]
public class PlayerLocomotionBrain : MonoBehaviour
{
    private PlayerInputProxy inputProxy;

    [SerializeField] private bool movementEnabled = true;
    [SerializeField] private bool jumpEnabled = true;
    [SerializeField] private bool lookEnabled = true;
    [SerializeField] private bool sprintEnabled = true;
    [SerializeField] private bool attackEnabled = true;
    [SerializeField] private bool stopEnabled = true;
    

    public PlayerControlCommand CurrentCommand
    {
        get
        {
            if (inputProxy == null)
            {
                inputProxy = GetComponent<PlayerInputProxy>();
            }

            var cmd = inputProxy != null ? inputProxy.CurrentCommand : PlayerControlCommand.Empty;

            if (!movementEnabled)
            {
                cmd.move = Vector2.zero;
            }

            if (!jumpEnabled)
            {
                cmd.jump = false;
            }

            if (!lookEnabled)
            {
                cmd.look = Vector2.zero;
            }

            if (!sprintEnabled)
            {
                cmd.sprint = false;
            }

            return cmd;
        }
    }

    private void Awake()
    {
        inputProxy = GetComponent<PlayerInputProxy>();
    }

    public void ConsumeJump()
    {
        inputProxy?.ConsumeJump();
    }

    public void SetMovementEnabled(bool enabled)
    {
        movementEnabled = enabled;
        if (!enabled && inputProxy != null)
        {
            // 只清导航覆盖，手动原始输入不在这里修改
            // 最终输出会因为 movementEnabled=false 而被置零
        }
    }

    public void SetJumpEnabled(bool enabled)
    {
        jumpEnabled = enabled;
        if (!enabled)
        {
            inputProxy?.ConsumeJump();
        }
    }

    public void SetLookEnabled(bool enabled)
    {
        lookEnabled = enabled;
    }

    public void SetSprintEnabled(bool enabled)
    {
        sprintEnabled = enabled;
    }

    public void SetFullControlEnabled(bool enabled)
    {
        movementEnabled = enabled;
        jumpEnabled = enabled;
        lookEnabled = enabled;
        sprintEnabled = enabled;

        if (!enabled)
        {
            inputProxy?.ConsumeJump();
        }
    }

    public bool IsMovementEnabled() => movementEnabled;
    public bool IsJumpEnabled() => jumpEnabled;
    public bool IsLookEnabled() => lookEnabled;
    public bool IsSprintEnabled() => sprintEnabled;
}
