using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Deadlands.Nomad;

internal static class NomadGraphics
{
    private static ConditionalWeakTable<Player, NomadEX> _slideData;
    
    private static readonly ConditionalWeakTable<Player, Wings> Wings = new();
    private static readonly ConditionalWeakTable<Player, Whiskers> Whiskers = new();

    private static readonly Color NomadColor = new(1f, 196f / 255f, 120f / 255f, 1);

    private static bool _initializingSprites = false;
    
    
    public static int MSC => ModManager.MSC? 1 : 0;

    /////////////////////////////////////
    // Initialization
    /////////////////////////////////////
    
    public static void OnInit(ConditionalWeakTable<Player, NomadEX> slideData)
    {
        _slideData = slideData;
        
        
        On.PlayerGraphics.ctor += (orig, self, ow) =>
        {
            orig(self, ow);
            if (!_slideData.TryGetValue(self.player, out var playerData) || !playerData.isNomad) return;

            Wings.Add(self.player, new Wings(self, 12 + (ModManager.MSC ? 1 : 0)));
            // Whiskers.Add(self.player, new Whiskers(self, 14 + (ModManager.MSC ? 1 : 0)));


            if (self.owner.room.world.game.IsArenaSession) return;

            ((PlayerState)self.player.State).slugcatCharacter = Plugin.Name;
            self.player.SlugCatClass = Plugin.Name;
        };

        /////////////////////////////////////
        // Changing the nomad's color
        /////////////////////////////////////
        
        On.PlayerGraphics.DefaultSlugcatColor += (orig, slugClass) => slugClass == Plugin.Name ? NomadColor : orig(slugClass);

        /////////////////////////////////////
        // Drawing
        /////////////////////////////////////
        
        On.PlayerGraphics.InitiateSprites += InititateSprites;
        On.PlayerGraphics.DrawSprites += DrawSprites;
        On.PlayerGraphics.ApplyPalette += ApplyPalette;
        On.PlayerGraphics.AddToContainer += AddToContainer;
        
        /////////////////////////////////////
        // Wing splay
        /////////////////////////////////////

        On.SlugcatHand.EngageInMovement += SlugcatHandOnEngageInMovement;
    }
    
    
    
    
    
    private static void InititateSprites(
        On.PlayerGraphics.orig_InitiateSprites orig,
        PlayerGraphics self,
        RoomCamera.SpriteLeaser sLeaser,
        RoomCamera rCam
    )   {   
        _initializingSprites = true;
        orig(self, sLeaser, rCam);
        _initializingSprites = false;

        if (!_slideData.TryGetValue(self.player, out var playerData) || !playerData.isNomad) return;



        Array.Resize(ref sLeaser.sprites, 14 + (ModManager.MSC ? 1 : 0));

        if (Wings.TryGetValue(self.player, out var wings)) 
            wings.InitiateSprites(sLeaser, rCam);

        // if (Whiskers.TryGetValue(self.player, out var whiskers))
            // whiskers.InitiateSprites(sLeaser, rCam);
        
        self.AddToContainer(sLeaser, rCam, null);
    }

    private static void DrawSprites(
        On.PlayerGraphics.orig_DrawSprites orig,
        PlayerGraphics self,
        RoomCamera.SpriteLeaser sLeaser,
        RoomCamera rCam,
        float timeStacker,
        Vector2 camPos
    )   {
        orig(self, sLeaser, rCam, timeStacker, camPos);
        if (!_slideData.TryGetValue(self.owner as Player, out var playerData) || !playerData.isNomad) return;



        if (self.player.room == null) return;

        if (Wings.TryGetValue(self.player, out var wings))
            wings.DrawSprites(sLeaser, rCam, timeStacker, camPos);
        
        // if (Whiskers.TryGetValue(self.player, out var whiskers))
            // whiskers.DrawSprites(sLeaser, rCam, timeStacker, camPos);
    }

    private static void ApplyPalette(
        On.PlayerGraphics.orig_ApplyPalette orig, 
        PlayerGraphics self, 
        RoomCamera.SpriteLeaser sLeaser, 
        RoomCamera rCam, 
        RoomPalette palette
    )   {
        orig(self, sLeaser, rCam, palette);
        if (!_slideData.TryGetValue(self.owner as Player, out var playerData) || !playerData.isNomad) return;


        if (!rCam.room.world.game.IsArenaSession)
        {
            for (int i = 0; i < 14 + (ModManager.MSC ? 1 : 0); i++)
            {
                if (i == 9) continue;
            
                sLeaser.sprites[i].color = PlayerGraphics.SlugcatColor((self.owner as Player).SlugCatClass);
            }
        }

        if (Wings.TryGetValue(self.player, out var wings))
            wings.ApplyPalette(sLeaser, rCam, palette);
        
        // if (Whiskers.TryGetValue(self.player, out var whiskers))
            // whiskers.ApplyPalette(sLeaser, rCam, palette);
    }

    private static void AddToContainer(
        On.PlayerGraphics.orig_AddToContainer orig,
        PlayerGraphics self,
        RoomCamera.SpriteLeaser sLeaser,
        RoomCamera rCam,
        FContainer newContainer
    )   {
        orig(self, sLeaser, rCam, newContainer);
        
        if (_initializingSprites) return;
        
        
        if (!_slideData.TryGetValue(self.owner as Player, out var playerData) || !playerData.isNomad) return;

        if (Wings.TryGetValue(self.player, out var wings))
            wings.AddToContainer(sLeaser, rCam, rCam.ReturnFContainer("Midground"));
        
        // if (Whiskers.TryGetValue(self.player, out var whiskers)) 
            // whiskers.AddToContainer(sLeaser, rCam, rCam.ReturnFContainer("Midground"));
    }
    
    



    private static bool SlugcatHandOnEngageInMovement(On.SlugcatHand.orig_EngageInMovement orig, SlugcatHand self)
    {
        Player player = self.owner.owner as Player;

        ///////////////////////////////////////////////////////////
        // Checks (Don't do wing splay if any of these are true)
        ///////////////////////////////////////////////////////////

        if (!_slideData.TryGetValue(player, out var playerData) || !playerData.isNomad) return orig(self);

        if (!playerData.isSliding) return orig(self); // Sliding? Gliding? :lizblackbruh:
        if (!player.CanGlide()) return orig(self);


        if (player.animation == Player.AnimationIndex.BellySlide ||
            player.animation == Player.AnimationIndex.RocketJump ||
            player.animation == Player.AnimationIndex.Flip ||
            player.animation == Player.AnimationIndex.Roll ||
            player.animation == Player.AnimationIndex.ClimbOnBeam) return orig(self); // The player shouldn't spread their wings during any of these animations


        var standing = player.bodyMode == Player.BodyModeIndex.Stand;

        if (Mathf.Abs(player.firstChunk.vel.x) > 0.1f && standing) return orig(self); // Wing splay while walking looks funky, so don't

        ///////////////////////////////////////////////////////////
        // Try to make the hand as disabled as possible
        ///////////////////////////////////////////////////////////

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
        var player_orientation = player.bodyChunks[0].pos - player.bodyChunks[1].pos; // A line from the top, to bottom of the player

        Vector2 targetPos = 
            52 * // The vector's amplitude
            (self.limbNumber - 0.5f) * // Differentiate left and right hands
            new Vector2(player_orientation.y, -player_orientation.x).normalized; // The player's orientation rotated 90 degrees

        targetPos += (standing ? 7f : -1f) * player_orientation.normalized; // Move downward relative to player orientation

        // Assignment
        self.relativeHuntPos = targetPos;
        self.pos = Vector2.Lerp(self.pos, player.firstChunk.pos + targetPos, 0.5f);


        return orig(self);
    }
}
