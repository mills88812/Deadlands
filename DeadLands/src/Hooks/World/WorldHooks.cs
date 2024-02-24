using Deadlands.Enums;
using Deadlands.Features;
using Deadlands.Features.Weather;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Deadlands.Hooks.World
{
    /// <summary>
    /// Probably horribly unoptimized but that can come later.
    /// </summary>
    internal class WorldHooks
    {
        public static ConditionalWeakTable<Room, RoomWeatherModule> RoomWeatherModules = new();

        public static string[] TESTS = new string[4];
        public static void Apply()
        {
            // Room

            On.Room.Loaded += OnRoomLoaded;

            On.Room.NoLongerViewed += (orig, self) =>
            {
                orig(self);
                RoomWeatherModules.TryGetValue(self, out var weatherModule);
                if (weatherModule != null)
                {
                    weatherModule.roomSandstorm = null;
                }
            };

            // RoomCamera

            On.RoomCamera.ChangeRoom += OnChangeRoom;

            On.RoomCamera.DrawUpdate += OnDrawUpdate;

            IL.RoomCamera.Update += ILRoomCameraUpdate;

            // RoomRain

            //On.RoomRain.

            // Debug

            On.AbstractSpaceVisualizer.Update += (orig, self) =>
            {
                orig(self);
                if (self.room != null && self.room.roomSettings.DangerType == DeadlandsEnums.Desert || self.room.roomSettings.DangerType == DeadlandsEnums.Sandstorm || self.room.roomSettings.DangerType == DeadlandsEnums.DesertAndSandstorm)
                {
                    string str = "\n\n";
                    RoomWeatherModules.TryGetValue(self.room, out var module);

                    str += "Deadlands Weather:\n";
                    if (module == null)
                    {
                        str += "RoomWeatherModule not found!";
                        self.infoText.text += str;
                        return;
                    }

                    str += "tests: " + TESTS[0] + " | "  + TESTS[1] + " | " + TESTS[2] + " | " + TESTS[3] + "\n";
                    str += $"rSS: {module.roomSandstorm}\n";
                    if (module.roomSandstorm != null)
                    {
                        str += $"mPos: {module.roomSandstorm.mPos}\n";
                        str += $"prog: {module.roomSandstorm.Progress}\n";
                        str += $"inty: {module.roomSandstorm.Intensity}\n";
                        str += $"rInty: {module.roomSandstorm.room.roomSettings.RainIntensity}\n";
                    }

                    self.infoText.text += str;
                }
            };
        }

        private static void OnDrawUpdate(On.RoomCamera.orig_DrawUpdate orig, RoomCamera self, float timeStacker, float timeSpeed)
        {
            if (self.room != null)
            {
                RoomWeatherModules.TryGetValue(self.room, out var weatherModule);
                if (weatherModule != null)
                {
                    weatherModule.CameraDrawUpdate(self);
                } else
                {
                    TESTS[3] = "RoomWeatherModule not found for " + self.room;
                }
            }
            orig(self, timeStacker, timeSpeed);
        }

        private static void ILRoomCameraDrawUpdate(ILContext il)
        {
            var c = new ILCursor(il);

            if (c.TryGotoNext(MoveType.After,
                x => x.MatchLdarg(0),
                x => x.MatchCall(typeof(Screen).GetMethod("get_fullScreen")),
                x => x.MatchStfld(typeof(RoomCamera).GetField("fullscreenSync"))))
            {
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate<Action<RoomCamera>>(rCam =>
                {
                    
                });
            }
        }

        private static void OnChangeRoom(On.RoomCamera.orig_ChangeRoom orig, RoomCamera self, Room newRoom, int cameraPosition)
        {
            orig(self, newRoom, cameraPosition);
            if (self.room != null)
            {
                RoomWeatherModules.TryGetValue(self.room, out var weatherModule);
                if (weatherModule != null && weatherModule.sandstorm != null && weatherModule.roomSandstorm == null)
                {
                    Debug.Log("Pass Sandstorm");
                    self.room.AddObject(weatherModule.sandstorm);
                    weatherModule.roomSandstorm = weatherModule.sandstorm;
                }
            }
        }

        private static void ILRoomCameraUpdate(ILContext il)
        {
            var c = new ILCursor(il);

            if (c.TryGotoNext(MoveType.After,
                x => x.MatchLdarg(0),
                x => x.MatchLdfld(typeof(RoomCamera).GetField("shortcutGraphics")),
                x => x.MatchCallvirt(typeof(ShortcutGraphics).GetMethod("Update"))))
            {
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate<Action<RoomCamera>>(rCam =>
                {
                    if (rCam.room != null)
                    {
                        RoomWeatherModules.TryGetValue(rCam.room, out var weatherModule);
                        if (weatherModule != null)
                        {
                            weatherModule.CameraUpdate(rCam);
                        }
                    }
                });
            } 
        }

        private static void OnRoomLoaded(On.Room.orig_Loaded orig, Room self)
        {
            orig(self);
            if (self.game == null) return;
            for (int i = 0; i < self.roomSettings.effects.Count; i++)
            {
                if (self.roomSettings.effects[i].type == DeadlandsEnums.SunShade)
                {
                    self.AddObject(new SunShade(self));
                }
            }
            if (self.roomSettings.DangerType == DeadlandsEnums.Desert || self.roomSettings.DangerType == DeadlandsEnums.Sandstorm || self.roomSettings.DangerType == DeadlandsEnums.DesertAndSandstorm)
            {
                Debug.Log("Desert Danger Type: " + self.abstractRoom.name);
                RoomWeatherModules.Add(self, new RoomWeatherModule(self));
            }
        }
    }
}
