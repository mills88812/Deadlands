namespace Deadlands;

public static class DataPearlHooks
{
    public static void Apply()
    {
        // DataPearl

        On.DataPearl.UniquePearlMainColor += OnUniquePearlMainColor;
        On.DataPearl.UniquePearlHighLightColor += OnUniquePearlHighLightColor;

        On.DataPearl.ApplyPalette += OnApplyPalette;

        // Conversation

        On.Conversation.DataPearlToConversation += OnDataPearlToConversation;
    }

    private static Conversation.ID OnDataPearlToConversation(On.Conversation.orig_DataPearlToConversation orig, DataPearl.AbstractDataPearl.DataPearlType type)
    {
        if (type == DeadlandsEnums.DataPearlType.UPGoodbye)
        {
            return DeadlandsEnums.ConversationID.Moon_Pearl_UPGoodbye;
        }
        return orig(type);
    }

    private static void OnApplyPalette(On.DataPearl.orig_ApplyPalette orig, DataPearl self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        orig(self, sLeaser, rCam, palette);
        if (((DataPearl.AbstractDataPearl)self.abstractPhysicalObject).dataPearlType == DeadlandsEnums.DataPearlType.UPGoodbye)
        {
            self.color = DataPearl.UniquePearlMainColor(((DataPearl.AbstractDataPearl)self.abstractPhysicalObject).dataPearlType);
            self.highlightColor = DataPearl.UniquePearlHighLightColor(((DataPearl.AbstractDataPearl)self.abstractPhysicalObject).dataPearlType);
        }
    }

    private static Color? OnUniquePearlHighLightColor(On.DataPearl.orig_UniquePearlHighLightColor orig, DataPearl.AbstractDataPearl.DataPearlType pearlType)
    {
        if (pearlType == DeadlandsEnums.DataPearlType.UPGoodbye)
        {
            return Custom.HSL2RGB(0.82f, 0.81f, 0.53f);
        }
        return orig(pearlType);
    }

    private static Color OnUniquePearlMainColor(On.DataPearl.orig_UniquePearlMainColor orig, DataPearl.AbstractDataPearl.DataPearlType pearlType)
    {
        if (pearlType == DeadlandsEnums.DataPearlType.UPGoodbye)
        {
            return Custom.HSL2RGB(0.8f, 0.99f, 0.39f);
        }
        return orig(pearlType);
    }
}