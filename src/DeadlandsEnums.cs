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
    public static void RegisterEnums()
    {
        DLCreature.RegisterValues();
        MaskType.RegisterValues();
    }
    public static void Unregister()
    {
        Utils.UnregisterEnums(typeof(DangerType));
        Utils.UnregisterEnums(typeof(Sound));
        Utils.UnregisterEnums(typeof(DataPearlType));
        Utils.UnregisterEnums(typeof(ConversationID));
        Utils.UnregisterEnums(typeof(RoomEffect));
        DLCreature.UnregisterValues();
        MaskType.UnregisterValues();
    }
    public static void UnregisterEnums()
    {
        
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
public class DLCreature
{
    public static CreatureTemplate.Type Buzzard;
    public static CreatureTemplate.Type Iguana;
    public static CreatureTemplate.Type BrownLizard;
    public static CreatureTemplate.Type GlowLizard;
    public static CreatureTemplate.Type CandleMouse;
    public static CreatureTemplate.Type SpinePlant;
    public static CreatureTemplate.Type SaltWorm;
    public static void RegisterValues()
    {
        Buzzard = new CreatureTemplate.Type("Buzzard", true);
        Iguana = new CreatureTemplate.Type("IguanaLizard", true);
        BrownLizard = new CreatureTemplate.Type("BrownLizard", true);
        GlowLizard = new CreatureTemplate.Type("GlowLizard", true);
        // CandleMouse = new CreatureTemplate.Type("CandleMouse", true);
    }
    public static void UnregisterValues()
    {
        if (Buzzard != null)
        {
            Buzzard.Unregister();
        }
        if (Iguana != null)
        {
            Iguana.Unregister();
        }
        if (BrownLizard != null)
        {
            BrownLizard.Unregister();
        }
        if (GlowLizard != null)
        {
            GlowLizard.Unregister();
        }
        /*
        if (CandleMouse != null)
        {
            CandleMouse.Unregister();
        }
        */
    }
}

public class UnlockID
{
    public static MultiplayerUnlocks.SandboxUnlockID Buzzard;
    public static MultiplayerUnlocks.SandboxUnlockID Iguana;
    public static MultiplayerUnlocks.SandboxUnlockID BrownLizard;
    public static MultiplayerUnlocks.SandboxUnlockID GlowLizard;
    public static MultiplayerUnlocks.SandboxUnlockID CandleMouse;
    public static MultiplayerUnlocks.SandboxUnlockID SpinePlant;
    public static MultiplayerUnlocks.SandboxUnlockID SaltWorm;
    public static void RegisterValues()
    {
        Buzzard = new MultiplayerUnlocks.SandboxUnlockID("Buzzard", true);
        Iguana = new MultiplayerUnlocks.SandboxUnlockID("IguanaLizard", true);
        BrownLizard = new MultiplayerUnlocks.SandboxUnlockID("BrownLizard", true);
        GlowLizard = new MultiplayerUnlocks.SandboxUnlockID("GlowLizard", true);
        // CandleMouse = new CreatureTemplate.Type("CandleMouse", true);
    }
    public static void UnregisterValues()
    {
        if (Buzzard != null)
        {
            Buzzard.Unregister();
        }
        if (Iguana != null)
        {
            Iguana.Unregister();
        }
        if (BrownLizard != null)
        {
            BrownLizard.Unregister();
        }
        if (GlowLizard != null)
        {
            GlowLizard.Unregister();
        }
        /*
        if (CandleMouse != null)
        {
            CandleMouse.Unregister();
        }
        */
    }
}

public class MaskType
{
    public static VultureMask.MaskType BUZZARD;

    public static void RegisterValues()
    {
        BUZZARD = new VultureMask.MaskType("BUZZARD", true);
    }
    public static void UnregisterValues()
    {
        if (BUZZARD != null)
        {
            BUZZARD.Unregister();
        }
    }
}