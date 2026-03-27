public class PlayerStaminaChangedEvent
{
    public int currentStamina;
    public int maxStamina;

    public PlayerStaminaChangedEvent(int c, int m)
    {
        currentStamina = c;
        maxStamina = m;
    }
}
