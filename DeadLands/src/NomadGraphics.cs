//using ImprovedInput;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Net.NetworkInformation;

namespace Deadlands;

internal class NomadGraphics
{
    public static ConditionalWeakTable<Player, NomadEX> SlideData = new();

    public static void OnInit(ConditionalWeakTable<Player, NomadEX> slideData)
    {
        On.Player.InitiateGraphicsModule += Player_Init;
        On.SlugcatHand.EngageInMovement += SlugcatHandOnEngageInMovement;

        SlideData = slideData;
    }

    private static void Player_Init(On.Player.orig_InitiateGraphicsModule orig, Player self)
    {
        orig(self);

        if (!SlideData.TryGetValue(self, out var playerData) || !playerData.isNomad) return;

        for (int i = 0; i < 2; i++)
        {
            WingAbstract wing = new WingAbstract(
                self.room.world,
                self.abstractCreature.pos,
                self.room.game.GetNewID()
            );


            wing.pos.Tile = new(0, 0);

            wing.RealizeInRoom();
            wing.realizedObject.PlaceInRoom(self.room);

            (wing.realizedObject as Wing).player = self;
            (wing.realizedObject as Wing).index = i;

            (wing.realizedObject as Wing).wingColor = new Color(1f, 196f / 255f, 120f / 255f, 1);
        }

        /*WhiskerAbstract whisker = new WhiskerAbstract(
            self.room.world,
            self.abstractCreature.pos,
            self.room.game.GetNewID()
        );

        whisker.RealizeInRoom();
        whisker.realizedObject.PlaceInRoom(self.room);

        (whisker.realizedObject as Whisker).player = self;

        (whisker.realizedObject as Whisker).whiskerColor = Color.white;*/
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

        targetPos += (standing ? 7f : 3f) * player_orientation.normalized;

        // Assignment
        self.relativeHuntPos = targetPos;
        self.pos = Vector2.Lerp(self.pos, player.firstChunk.pos + targetPos, 60f * Time.deltaTime);


        return orig(self);
    }
}
