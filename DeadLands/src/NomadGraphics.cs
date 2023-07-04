//using ImprovedInput;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Net.NetworkInformation;
using IL.MoreSlugcats;
using System.Reflection;
using UnityEngine.UI;
using System.Drawing.Printing;

namespace Deadlands;

internal class NomadGraphics
{
    public static ConditionalWeakTable<Player, NomadEX> SlideData = new();
    public static Wings wings;

    public static Color nomadColor = new Color(1f, 196f / 255f, 120f / 255f, 1);

    private static bool initializing = false;


    public static void OnInit(ConditionalWeakTable<Player, NomadEX> slideData)
    {
        On.Player.InitiateGraphicsModule += (orig, self) =>
        {
            orig(self);

            if (!SlideData.TryGetValue(self, out var playerData) || !playerData.isNomad) return;

            wings = new Wings(self.graphicsModule as PlayerGraphics, 13);
        };

        On.PlayerGraphics.InitiateSprites += InititateSprites;

        On.PlayerGraphics.DrawSprites += DrawSprites;
        On.PlayerGraphics.ApplyPalette += ApplyPalette;
        On.PlayerGraphics.AddToContainer += AddToContainer;

        On.SlugcatHand.EngageInMovement += SlugcatHandOnEngageInMovement;


        SlideData = slideData;
    }


    private static void InititateSprites(On.PlayerGraphics.orig_InitiateSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        initializing = true;
        orig(self, sLeaser, rCam);
        initializing = false;

        if (!SlideData.TryGetValue(self.owner as Player, out var playerData) || !playerData.isNomad) return;


        Array.Resize<FSprite>(ref sLeaser.sprites, 15);
        wings.InitiateSprites(sLeaser, rCam);

        self.AddToContainer(sLeaser, rCam, null);
    }

    private static void DrawSprites(On.PlayerGraphics.orig_DrawSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        orig(self, sLeaser, rCam, timeStacker, camPos);

        if (!SlideData.TryGetValue(self.owner as Player, out var playerData) || !playerData.isNomad) return;


        if (self.player.room == null) return;


        //foreach (var item in sLeaser.sprites) item.color = nomadColor;
        //sLeaser.sprites[9].color = Color.green; // Setting the face color

        wings.DrawSprites(sLeaser, rCam, timeStacker, camPos);
    }

    private static void ApplyPalette(On.PlayerGraphics.orig_ApplyPalette orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        orig(self, sLeaser, rCam, palette);

        if (!SlideData.TryGetValue(self.owner as Player, out var playerData) || !playerData.isNomad) return;


        wings.ApplyPalette(sLeaser, rCam, palette);
    }

    private static void AddToContainer(On.PlayerGraphics.orig_AddToContainer orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContainer)
    {
        orig(self, sLeaser, rCam, newContainer);

        if (!SlideData.TryGetValue(self.owner as Player, out var playerData) || !playerData.isNomad) return;


        if (initializing) return;

        wings.AddToContainer(sLeaser, rCam, rCam.ReturnFContainer("Midground"));
    }



    private static bool SlugcatHandOnEngageInMovement(On.SlugcatHand.orig_EngageInMovement orig, SlugcatHand self)
    {
        Player player = self.owner.owner as Player;

        ///////////////////////////////////////////////////////////
        // Checks and stuff
        ///////////////////////////////////////////////////////////

        if (!SlideData.TryGetValue(player, out var playerData) || !playerData.isNomad) return orig(self);

        if (!playerData.isSliding) return orig(self); // Sliding? Gliding? :lizblackbruh:
        if (!HelperFuncs.CanGlide(player)) return orig(self);

        // The player shouldn't spread their wings during any of these animations

        if (player.animation == Player.AnimationIndex.BellySlide ||
            player.animation == Player.AnimationIndex.RocketJump ||
            player.animation == Player.AnimationIndex.Flip ||
            player.animation == Player.AnimationIndex.Roll ||
            player.animation == Player.AnimationIndex.ClimbOnBeam) return orig(self);

        var standing = player.bodyMode == Player.BodyModeIndex.Stand;

        if (Mathf.Abs(player.firstChunk.vel.x) > 0.1f && standing) return orig(self); // Wing splay while walking looks funky, so don't do it if walking


        // Trying to make the hand as disabled as possible

        self.mode = Limb.Mode.Dangle;
        self.huntSpeed = 0f;
        self.quickness = 0f;
        self.retractCounter = 1;
        self.reachingForObject = false;
        self.retract = false;
        self.vel = Vector2.zero;

        ///////////////////////////////////////////////////////////
        // Hand position calculation and assignment
        ///////////////////////////////////////////////////////////

        // Calculation
        var player_orientation = player.bodyChunks[0].pos - player.bodyChunks[1].pos;

        Vector2 targetPos = 52 * (self.limbNumber - 0.5f) *
            new Vector2(player_orientation.y, -player_orientation.x).normalized;

        targetPos += (standing ? 7f : 0f) * player_orientation.normalized;

        // Assignment
        self.relativeHuntPos = targetPos;
        self.pos = Vector2.Lerp(self.pos, player.firstChunk.pos + targetPos, 60f * Time.deltaTime);
        return orig(self);
    }
}
