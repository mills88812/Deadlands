using System;
using BepInEx;
using UnityEngine;
using ImprovedInput;
using SlugBase.Features;
using static SlugBase.Features.FeatureTypes;
using System.Xml.Schema;
using DressMySlugcat;
using IL;
using System.Linq;
using On;
using System.Collections.Generic;
using SlugBase;

namespace SlugTemplate
{
    [BepInPlugin(MOD_ID, "DeadLands", "0.1.0")]
    class Plugin : BaseUnityPlugin
    {
        private const string MOD_ID = "mills888.DeadLands";

        public static readonly PlayerFeature<bool> Glide = PlayerBool("dead/glide");
        public static readonly PlayerKeybind GlideKey = PlayerKeybind.Register("SlugTemplate:GlideKey", "GlideKey", "GlideKey", KeyCode.Z, KeyCode.Joystick1Button0);

        Player.InputPackage Glidekey = new Player.InputPackage();





        [module: UnverifiableCode]
#pragma warning disable CS0618 // Type or member is obsolete
        [assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete


        // Add hooks
        public void OnEnable()
        {
       

            On.RainWorld.OnModsInit += Extras.WrapInit(LoadResources);
            On.RainWorld.PostModsInit += RainWorld_PostModsInit;
            // Put your custom hooks here!
            On.Player.Update += Player_Update;


        }


        public static bool IsPostInit;
        private void RainWorld_PostModsInit(On.RainWorld.orig_PostModsInit orig, RainWorld self)
        {
            orig(self);
            try
            {
                if (IsPostInit) return;
                IsPostInit = true;

                //-- Creating a blank slugbase slugcat, you don't need this if you are using the json
                var templateCat = SlugBaseCharacter.Create("dmstemplatecat");
                templateCat.DisplayName = "Template Cat";
                templateCat.Description = "Example of how to set up slugbase integration with DMS";

                //-- You can have the DMS sprite setup in a separate method and only call it if DMS is loaded
                //-- With this the mod will still work even if DMS isn't installed
                if (ModManager.ActiveMods.Any(mod => mod.id == "dressmyslugcat"))
                {
                    SetupDMSSprites();
                }

                Debug.Log($"Plugin dressmyslugcat.templatecat is loaded!");
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }





        public void SetupDMSSprites()
        {
            //-- The ID of the spritesheet we will be using as the default sprites for our slugcat


            for (int i = 0; i < 4; i++)
            {
                SpriteDefinitions.AddSlugcatDefault(new Customization()
                {
                    //-- Make sure to use the same ID as the one used for our slugcat
                    Slugcat = "nomad",
                    PlayerNumber = i,
                    CustomSprites = new List<CustomSprite>
                    {
                        //-- You can customize which spritesheet and color each body part will use
                        new CustomSprite() { Sprite = "HEAD", SpriteSheetID = "Perle_gel.Nomad" },
                        new CustomSprite() { Sprite = "FACE", SpriteSheetID = "Perle_gel.Nomad", Color = Color.red },
                        new CustomSprite() { Sprite = "BODY", SpriteSheetID = "Perle_gel.Nomad" },
                        new CustomSprite() { Sprite = "ARMS", SpriteSheetID = "Perle_gel.Nomad" },
                        new CustomSprite() { Sprite = "HIPS", SpriteSheetID = "Perle_gel.Nomad" },
                        new CustomSprite() { Sprite = "TAIL", SpriteSheetID = "Perle_gel.Nomad" }
                    }




                });

             }
        }


        // Load any resources, such as sprites or sounds
        private void LoadResources(RainWorld rainWorld)
        {
        }
        private void Player_Update(On.Player.orig_Update orig, Player self, bool eu)
        {
            orig(self, eu);
            if(Glide.TryGet(self, out bool flight) && flight && self.bodyMode != Player.BodyModeIndex.Stand && self.bodyMode != Player.BodyModeIndex.Crawl && self.bodyMode != Player.BodyModeIndex.WallClimb && self.bodyMode != Player.BodyModeIndex.CorridorClimb && self.bodyMode != Player.BodyModeIndex.ClimbingOnBeam && self.bodyMode != Player.BodyModeIndex.ClimbIntoShortCut && self.bodyMode != Player.BodyModeIndex.ZeroG && self.bodyChunks[0].vel.y <= 0f)
            {
                if (self.IsPressed(GlideKey))
                {

                    //body  part 1 move in y position
                    self.bodyChunks[0].vel.y *= 0.7f;
                    self.bodyChunks[0].vel.y += 0.0008f;
                    //body part 2
                    self.bodyChunks[1].vel.y *= 0.7f;
                    self.bodyChunks[1].vel.y += 0.0008f;

                    //move in x position when you move  a certain speed
                    if (self.bodyChunks[0].vel.x <= 0)
                    {
                        self.bodyChunks[0].vel.x += -0.2f;
                    }
                    else if (self.bodyChunks[0].vel.x >= 0)
                    {
                        self.bodyChunks[0].vel.x += 0.2f;
                    }

                }
            }
            
        }
    }
}
