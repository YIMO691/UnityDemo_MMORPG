using UnityEngine;

public struct ActorControlCommand
{
    public Vector3 moveDirection;
    public Vector3 lookDirection;
    public bool jump;
    public bool sprint;
    public bool attack;
    public bool stop;

    public static ActorControlCommand Empty => new ActorControlCommand
    {
        moveDirection = Vector3.zero,
        lookDirection = Vector3.zero,
        jump = false,
        sprint = false,
        attack = false,
        stop = false
    };
}
