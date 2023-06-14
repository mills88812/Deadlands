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
using Fisobs.Core;
using SlugTemplate;
using RWCustom;

namespace Deadlands;

[BepInPlugin(MOD_ID, nameof(Deadlands), "0.1.0")]
class Plugin : BaseUnityPlugin
{
    private const string MOD_ID = "mills888.DeadLands";

    public static readonly PlayerFeature<bool> Glide = PlayerBool("dead/glide");
    public static readonly PlayerKeybind GlideKey = PlayerKeybind.Register("SlugTemplate:GlideKey", "GlideKey", "GlideKey", KeyCode.Z, KeyCode.Joystick1Button0);

    public static readonly SlugcatStats.Name NomadName = new SlugcatStats.Name("Nomad", false);

    Player.InputPackage Glidekey = new Player.InputPackage();




    [module: UnverifiableCode]
#pragma warning disable CS0618 // Type or member is obsolete
    [assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete



    // Add hooks
    public void OnEnable()
    {
        //Content.Register(new WingFisob());

        On.RainWorld.OnModsInit += Extras.WrapInit(LoadResources);
        On.RainWorld.PostModsInit += RainWorld_PostModsInit;

        // Put your custom hooks here!

        On.Player.InitiateGraphicsModule += Player_Init;
        On.Player.Update += Player_Update;

        On.SlugcatHand.EngageInMovement += SlugcatHandOnEngageInMovement;
    }



    public static bool IsPostInit;

    private void RainWorld_PostModsInit(On.RainWorld.orig_PostModsInit orig, RainWorld self)
    {
        orig(self);
        try
        {
            if (IsPostInit) return;
            IsPostInit = true;

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
                Slugcat = "Nomad",
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


    private void Player_Init(On.Player.orig_InitiateGraphicsModule orig, Player self)
    {
        orig(self);

        for (int i = 0; i < 2; i++)
        {
            WingAbstract wing = new WingAbstract(
                self.room.world,
                self.abstractCreature.pos,
                self.room.game.GetNewID()
            );

            wing.pos.Tile = new RWCustom.IntVector2(0, 0);

            wing.RealizeInRoom();
            wing.realizedObject.PlaceInRoom(self.room);

            (wing.realizedObject as Wing).player = self;
            (wing.realizedObject as Wing).index = i;

            (wing.realizedObject as Wing).wingColor = new Color(1f, 196f / 255f, 120f / 255f, 1);
        }
    }

    private void Player_Update(On.Player.orig_Update orig, Player self, bool eu)
    {
        orig(self, eu);

        if (self.IsPressed(GlideKey))
        {
            if (
                Glide.TryGet(self, out bool flight) &&
                flight &&
                self.bodyMode != Player.BodyModeIndex.Stand &&
                HelperFuncs.CanGlide(self) &&
                self.bodyChunks[0].vel.y <= 0f
            )
            {
                // Body  part 1 move in y position
                self.bodyChunks[0].vel.y *= 0.7f;
                self.bodyChunks[0].vel.y += 0.0008f;

                // Body part 2
                self.bodyChunks[1].vel.y *= 0.7f;
                self.bodyChunks[1].vel.y += 0.0008f;

                // Move in x position when you move a certain speed
                //self.bodyChunks[0].vel.x += Mathf.Clamp(self.bodyChunks[0].vel.x, -0.2f, 0.2f);
            }
        }
    }



    private bool SlugcatHandOnEngageInMovement(On.SlugcatHand.orig_EngageInMovement orig, SlugcatHand self)
    {
        Player player = self.owner.owner as Player;

        if (!player.IsPressed(GlideKey)) return orig(self);

        if (!HelperFuncs.CanGlide(player)) return orig(self);

        if (player.animation == Player.AnimationIndex.BellySlide ||
            player.animation == Player.AnimationIndex.RocketJump ||
            player.animation == Player.AnimationIndex.Flip ||
            player.animation == Player.AnimationIndex.Roll ||
            player.animation == Player.AnimationIndex.ClimbOnBeam) return orig(self);

        if (Mathf.Abs(player.firstChunk.vel.x) > 0.1f &&
            player.bodyMode == Player.BodyModeIndex.Stand) return orig(self);

        self.mode = Limb.Mode.Dangle;
        self.huntSpeed = 0f;
        self.quickness = 0f;
        self.retractCounter = 1;
        self.reachingForObject = false;
        self.retract = false;
        self.vel = Vector2.zero;

        Vector2 targetPos = new Vector2((self.limbNumber - 0.5f) * 52f, -7f);

        self.relativeHuntPos = targetPos;
        self.pos = Vector2.Lerp(self.pos, player.firstChunk.pos + targetPos, 60f * Time.deltaTime);


        return orig(self);
    }
}

