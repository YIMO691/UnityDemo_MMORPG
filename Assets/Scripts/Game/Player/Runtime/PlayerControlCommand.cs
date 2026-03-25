using UnityEngine;

public struct PlayerControlCommand
{
    public Vector2 move;
    public Vector2 look;
    public bool jump;
    public bool sprint;

    public static PlayerControlCommand Empty => new PlayerControlCommand
    {
        move = Vector2.zero,
        look = Vector2.zero,
        jump = false,
        sprint = false
    };

    public static PlayerControlCommand FromActorCommand(ActorControlCommand cmd, Camera cam)
    {
        Vector3 forward = cam != null ? cam.transform.forward : Vector3.forward;
        Vector3 right = cam != null ? cam.transform.right : Vector3.right;
        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();
        float x = Vector3.Dot(cmd.moveDirection, right);
        float y = Vector3.Dot(cmd.moveDirection, forward);
        return new PlayerControlCommand
        {
            move = new Vector2(x, y),
            look = Vector2.zero,
            jump = cmd.jump,
            sprint = cmd.sprint
        };
    }

    public ActorControlCommand ToActorCommand(Camera cam)
    {
        Vector3 forward = cam != null ? cam.transform.forward : Vector3.forward;
        Vector3 right = cam != null ? cam.transform.right : Vector3.right;
        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();
        Vector3 moveDir = right * move.x + forward * move.y;
        moveDir.y = 0f;
        return new ActorControlCommand
        {
            moveDirection = moveDir,
            lookDirection = forward,
            jump = jump,
            sprint = sprint,
            attack = false,
            stop = false
        };
    }
}
