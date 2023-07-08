namespace Deadlands;

public class DangerTypeHeat
{
    public static RoomRain.DangerType Heat;

    public static void RegisterValues()
    {
        Heat = new RoomRain.DangerType("Heat", true);
    }

    public static void UnregisterValues()
    {
        if (Heat == null) return;
        
        Heat.Unregister();
        Heat = null;
    }
}