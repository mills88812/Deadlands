namespace Deadlands.Enums;

internal static class DeadlandsEnums
{
    public static SoundID Wind { get; private set; } = null!;

    //Hook to plugin cs
    public static void RegisterValues()
    {
        Wind = new SoundID("wind", true);
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