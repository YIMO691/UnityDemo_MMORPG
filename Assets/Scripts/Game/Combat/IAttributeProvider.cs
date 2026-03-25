public interface IAttributeProvider
{
    int MaxHp { get; }
    int CurrentHp { get; }
    float MoveSpeed { get; }

    int Attack { get; }
    int Defense { get; }

    float CritRate { get; }
    float CritDamage { get; }
    float HitRate { get; }
    float DodgeRate { get; }
}
