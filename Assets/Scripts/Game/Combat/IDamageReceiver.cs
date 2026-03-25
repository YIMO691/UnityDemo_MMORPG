public interface IDamageReceiver
{
    bool IsDead { get; }
    void ReceiveDamage(int damage);
}
