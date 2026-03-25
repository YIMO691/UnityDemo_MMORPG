using UnityEngine;

public interface IActorIdentity
{
    string RuntimeId { get; }
    Transform ActorTransform { get; }
    string DisplayName { get; }
    bool IsDead { get; }
}
