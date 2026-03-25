public class DeathEvent
{
    public IDamageReceiver deadEntity;
    public ICombatSource killer;

    public DeathEvent(IDamageReceiver deadEntity, ICombatSource killer)
    {
        this.deadEntity = deadEntity;
        this.killer = killer;
    }
}
