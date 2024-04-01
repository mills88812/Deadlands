namespace Deadlands;

public static class MenuHooks
{
    public static void Apply()
    {
        //On.Menu.SleepAndDeathScreen.ctor += OnSleepAndDeathScreen;

        On.Menu.SleepAndDeathScreen.GetDataFromGame += OnGetDataFromGame;

        //On.MenuMicrophone.PlayCustomLoop += OnPlayCustomLoop;
    }

    /// <summary>
    /// Changes the Sleep Screen sound depending on the region & shelter
    /// </summary>
    private static void OnGetDataFromGame(On.Menu.SleepAndDeathScreen.orig_GetDataFromGame orig, SleepAndDeathScreen self, KarmaLadderScreen.SleepDeathScreenDataPackage package)
    {
        orig(self, package);
        if (self.IsSleepScreen && IsDeadlandsRegion(self, package))
        {
            self.mySoundLoopID = SwapShelterSound(package);
        }
    }

    private static SoundID SwapShelterSound(KarmaLadderScreen.SleepDeathScreenDataPackage package)
    {
        SoundID sound = SoundID.MENU_Death_Screen_LOOP;
        Debug.Log("Deadlands current shelter: " + package.mapData.NameOfRoom(package.playerRoom));
        switch (package.mapData.NameOfRoom(package.playerRoom))
        {
            // Add shelters here that you want to play a different sound than normal,
            // shelters will be muted by default if its in a Deadlands region
            case "DL_S33":
                sound = DeadlandsEnums.Sound.basicmech;
                break;

            case "DL_S24":
                sound = SoundID.MENU_Sleep_Screen_LOOP;
                break;

            default: return sound;
        }
        return sound;
    }

    public static bool IsDeadlandsRegion(SleepAndDeathScreen self, KarmaLadderScreen.SleepDeathScreenDataPackage package)
    {
        // TODO: Check if the cycle is a death rain cycle (once that stuff is added)
        string[] regions = ["DL", "AL", "AU"];
        Debug.Log("Checking isDeadlandsRegion: " + package.mapData.regionName);
        return regions.Contains(package.mapData.regionName);
    }
}