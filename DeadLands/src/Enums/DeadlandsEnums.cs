namespace Deadlands.Enums;

internal static class DeadlandsEnums
{
    // SoundID
    public static SoundID Wind { get; private set; } = null!;
    public static SoundID Basic_Mech { get; private set; } = null!;

    // DataPearlType
    public static DataPearl.AbstractDataPearl.DataPearlType UPGoodbye { get; private set; } = null!;

    // Conversation
    public static Conversation.ID Moon_Pearl_UPGoodbye { get; private set; } = null!;
    public static Conversation.ID Pebbles_Pearl_UPGoodbye { get; private set; } = null!;

    //Hook to plugin cs
    public static void RegisterValues()
    {
        // SoundID
        Wind = new SoundID("wind", true);
        Basic_Mech = new SoundID("Basic_Mech", true);

        // DataPearlType
        UPGoodbye = new DataPearl.AbstractDataPearl.DataPearlType("UPGoodbye", true);

        // Conversation
        Moon_Pearl_UPGoodbye = new Conversation.ID("Moon_Pearl_UPGoodbye", true);
        Pebbles_Pearl_UPGoodbye = new Conversation.ID("Pebbles_Pearl_UPGoodbye", true);
    }
    /*
    // this function was (i assume) from some tutorial with the comment //DO NOT... therefore i removed it
    // (keeping it commented just in case its needed)
    public static void UnregisterValues(){Unregister(wind);}
    */
    private static void Unregister<T>(ExtEnum<T> extEnum) where T : ExtEnum<T>
    {
        extEnum?.Unregister();
    }
}