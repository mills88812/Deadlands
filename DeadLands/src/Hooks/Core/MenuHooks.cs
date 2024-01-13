using Deadlands.Enums;
using Menu;
using Rewired;
using System;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Deadlands.Hooks.Core
{
    internal class MenuHooks
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
        private static void OnGetDataFromGame(On.Menu.SleepAndDeathScreen.orig_GetDataFromGame orig, Menu.SleepAndDeathScreen self, Menu.KarmaLadderScreen.SleepDeathScreenDataPackage package)
        {
            orig(self, package);
            if (self.IsSleepScreen && isDeadlandsRegion(self, package))
            {
                self.mySoundLoopID = SwapShelterSound(self, package);
            }
        }

        private static SoundID SwapShelterSound(Menu.SleepAndDeathScreen self, KarmaLadderScreen.SleepDeathScreenDataPackage package)
        {
            SoundID sound = SoundID.MENU_Death_Screen_LOOP;
            Debug.Log("Deadlands current shelter: " + package.mapData.NameOfRoom(package.playerRoom));
            switch(package.mapData.NameOfRoom(package.playerRoom))
            {
                // Add shelters here that you want to play a different sound than normal,
                // shelters will be muted by default if its in a Deadlands region
                case "DL_S33":
                    sound = DeadlandsEnums.Basic_Mech;
                    break;
                case "DL_S24":
                    sound = SoundID.MENU_Sleep_Screen_LOOP;
                    break;
                    default: return sound;
            }
            return sound;
            
        }

        public static bool isDeadlandsRegion(Menu.SleepAndDeathScreen self, Menu.KarmaLadderScreen.SleepDeathScreenDataPackage package)
        {
            // TODO: Check if the cycle is a death rain cycle (once that stuff is added)
            string[] regions = { "DL", "AL", "AU" };
            Debug.Log("Checking isDeadlandsRegion: " + package.mapData.regionName);
            return regions.Contains(package.mapData.regionName);
        }

    }
}
