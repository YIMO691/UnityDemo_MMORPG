using UnityEngine;

[RequireComponent(typeof(StarterAssetsInputs))]
public class PlayerInputProxy : MonoBehaviour
{
    private StarterAssetsInputs rawInputs;

    private bool useNavigationOverride;
    private PlayerControlCommand navigationCommand;
    public bool IsUsingNavigation => useNavigationOverride;


    public PlayerControlCommand CurrentCommand
    {
        get
        {
            if (!useNavigationOverride)
                return ReadRawInput();

            var cmd = navigationCommand;
            if (rawInputs != null)
            {
                cmd.look = rawInputs.look;
            }
            return cmd;
        }
    }

    private void Awake()
    {
        rawInputs = GetComponent<StarterAssetsInputs>();
    }

    public void SetNavigationCommand(PlayerControlCommand command)
    {
        navigationCommand = command;
        useNavigationOverride = true;
    }

    public void ClearNavigationCommand()
    {
        navigationCommand = PlayerControlCommand.Empty;
        useNavigationOverride = false;
    }

    public void ConsumeJump()
    {
        if (rawInputs != null)
        {
            rawInputs.jump = false;
        }

        if (useNavigationOverride)
        {
            navigationCommand.jump = false;
        }
    }

    public bool HasManualMovementInput(float threshold = 0.01f)
    {
        if (rawInputs == null) return false;
        return rawInputs.move.sqrMagnitude > threshold;
    }

    public bool HasManualLookInput(float threshold = 0.01f)
    {
        if (rawInputs == null) return false;
        return rawInputs.look.sqrMagnitude > threshold;
    }

    public bool IsJumpPressed()
    {
        return rawInputs != null && rawInputs.jump;
    }

    public bool IsSprintPressed()
    {
        return rawInputs != null && rawInputs.sprint;
    }

    private PlayerControlCommand ReadRawInput()
    {
        if (rawInputs == null) return PlayerControlCommand.Empty;

        return new PlayerControlCommand
        {
            move = rawInputs.move,
            look = rawInputs.look,
            jump = rawInputs.jump,
            sprint = rawInputs.sprint
        };
    }
}
