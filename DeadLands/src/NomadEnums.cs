namespace Deadlands
{
    internal class NomadEnums
    {
        public static SoundID wind { get; set; }
        //Hook to plugin cs
        public static void RegisterValues()
        {
            wind = new SoundID("wind", true);
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
}
