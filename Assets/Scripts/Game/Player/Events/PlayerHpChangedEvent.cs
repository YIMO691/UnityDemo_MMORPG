public class PlayerHpChangedEvent
{
    public int currentHp;
    public int maxHp;

    public PlayerHpChangedEvent(int current, int max)
    {
        currentHp = current;
        maxHp = max;
    }
}
