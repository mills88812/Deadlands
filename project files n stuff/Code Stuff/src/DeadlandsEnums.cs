namespace Deadlands;

public static class DeadlandsEnums
{
    public static readonly SlugcatStats.Name Nomad = new("Nomad");

    public static void Init()
    {
        RuntimeHelpers.RunClassConstructor(typeof(DangerType).TypeHandle);
        RuntimeHelpers.RunClassConstructor(typeof(Sound).TypeHandle);
        RuntimeHelpers.RunClassConstructor(typeof(DataPearlType).TypeHandle);
        RuntimeHelpers.RunClassConstructor(typeof(ConversationID).TypeHandle);
        RuntimeHelpers.RunClassConstructor(typeof(RoomEffect).TypeHandle);
    }

    public static void Unregister()
    {
        Utils.UnregisterEnums(typeof(DangerType));
        Utils.UnregisterEnums(typeof(Sound));
        Utils.UnregisterEnums(typeof(DataPearlType));
        Utils.UnregisterEnums(typeof(ConversationID));
        Utils.UnregisterEnums(typeof(RoomEffect));
    }

    public static class DangerType
    {
        public static RoomRain.DangerType Desert; // Desert danger type covers heat and cold depending on time of day.
        public static RoomRain.DangerType DesertAndSandstorm; // Does both
        public static RoomRain.DangerType Sandstorm; // Sandstorms may be present
    }

    public static class Sound
    {
        public static SoundID wind;
        public static SoundID basicmech;
    }

    public static class DataPearlType
    {
        public static DataPearl.AbstractDataPearl.DataPearlType UPGoodbye;
    }

    public static class ConversationID
    {
        public static Conversation.ID Moon_Pearl_UPGoodbye;
        public static Conversation.ID Pebbles_Pearl_UPGoodbye;
    }

    public static class RoomEffect
    {
        public static RoomSettings.RoomEffect.Type SunShade;
    }
}