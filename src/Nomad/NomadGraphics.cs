using Fisobs.Creatures;
using UnityEngine.Assertions;

namespace Deadlands;

internal static class NomadGraphics
{
    /// <summary>
    /// This is the association between raw indices and <see cref="RoomCamera.SpriteLeaser.sprites"/>'s sprites. <br/>
    /// Leaving this here as reference and maybe a guide as to wtf is happening :^)
    /// </summary>
    /// <remarks> Check out <see cref="PlayerGraphics.InitiateSprites(RoomCamera.SpriteLeaser, RoomCamera)"/> to see where I got this info.</remarks> 
    enum SlugcatSpriteSlot : int
    {
        LowerBody = 0,
        Hips,
        MaybeTail,
        Head,
        Legs,
        LeftArm,
        RightArm,
        LeftCrawlHand,
        RightCrawlHand,
        Face = 9, // This one is usually always black, so should be treated special during any colouring.
        NeuronGlow, // I think? Might also be used for other situations where slugcats glow
        MarkOfCommunication
    }

    private static readonly ConditionalWeakTable<Player, Wings> Wings = new();
    private static readonly ConditionalWeakTable<Player, Whiskers> Whiskers = new();

    private static readonly Color NomadColor = new(1f, 196f / 255f, 120f / 255f, 1);

    private static bool _initializingSprites;

    /// <summary> How many sprites a slugcat normally needs to render, in Vanilla.</summary>
    private const int VanillaSlugcatSpriteCount = 12;

    /// <summary> How many additional sprite slots have to be reserved for sprites used by the More Slugcats DLC, if it's enabled.</summary>
    private static int MoreSlugsSpriteCount { get { return ModManager.MSC ? 1 : 0; } }

    /////////////////////////////////////
    // Initialization
    /////////////////////////////////////

    public static void Apply()
    {
        // Load the Nomad's sprites
        //Debug.Log("resource suffix is '" + Futile.resourceSuffix + "'");
        Futile.atlasManager.LoadAtlas("nomad/head");
        Futile.atlasManager.LoadAtlas("nomad/face");

        On.PlayerGraphics.ctor += (orig, self, ow) =>
        {
            orig(self, ow);
            if (!self.player.IsNomad(out _)) return;

            // Add the wings and whiskers of our boi, at the correct sprite indices
            Wings.Add(self.player, new Wings(self, VanillaSlugcatSpriteCount + MoreSlugsSpriteCount));
            Whiskers.Add(self.player, new Whiskers(self, VanillaSlugcatSpriteCount + MoreSlugsSpriteCount + Deadlands.Wings.RequiredSprites));
        };

        /////////////////////////////////////
        // Changing the nomad's color
        /////////////////////////////////////

        On.PlayerGraphics.DefaultSlugcatColor += (orig, slugClass) => slugClass == DeadlandsEnums.Nomad ? NomadColor : orig(slugClass);

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

    /////////////////////////////////////
    // Drawing
    /////////////////////////////////////

    private static void InititateSprites(On.PlayerGraphics.orig_InitiateSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        _initializingSprites = true;
        orig(self, sLeaser, rCam);
        _initializingSprites = false;

        if (!self.player.IsNomad(out _)) return;

        int totalSpritesNeeded = VanillaSlugcatSpriteCount + MoreSlugsSpriteCount + Deadlands.Wings.RequiredSprites + Deadlands.Whiskers.RequiredSprites;
        Array.Resize(ref sLeaser.sprites, totalSpritesNeeded);

        // Replace normal slugcat sprites with our cool ones
        sLeaser.sprites[(int)SlugcatSpriteSlot.Head] = new FSpriteNomad("nomad_HeadA0");
        sLeaser.sprites[(int)SlugcatSpriteSlot.Face] = new FSpriteNomad("nomad_FaceA0");

        if (Wings.TryGetValue(self.player, out var wings))
            wings.InitiateSprites(sLeaser, rCam);

        if (Whiskers.TryGetValue(self.player, out var whiskers))
            whiskers.InitiateSprites(sLeaser, rCam);

        self.AddToContainer(sLeaser, rCam, null);
    }

    private static void DrawSprites(On.PlayerGraphics.orig_DrawSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        orig(self, sLeaser, rCam, timeStacker, camPos);
        if (!(self.owner as Player)!.IsNomad(out var playerData)) return;

        if (self.player.room == null) return;

        if (Wings.TryGetValue(self.player, out var wings))
            wings.DrawSprites(sLeaser, timeStacker, camPos, playerData);

        if (Whiskers.TryGetValue(self.player, out var whiskers))
            whiskers.DrawSprites(sLeaser, rCam, timeStacker, camPos);
    }

    private static void ApplyPalette(On.PlayerGraphics.orig_ApplyPalette orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        orig(self, sLeaser, rCam, palette);
        if (!(self.owner as Player)!.IsNomad(out _)) return;

        if (!rCam.room.world.game.IsArenaSession)
        {
            for (int i = 0; i < 14 + (ModManager.MSC ? 1 : 0); i++)
            {
                if (i == (int)SlugcatSpriteSlot.Face) continue;

                sLeaser.sprites[i].color = PlayerGraphics.SlugcatColor((self.owner as Player)!.SlugCatClass);
            }
        }

        if (Wings.TryGetValue(self.player, out var wings))
            wings.ApplyPalette(sLeaser, rCam, palette);

        if (Whiskers.TryGetValue(self.player, out var whiskers))
            whiskers.ApplyPalette(sLeaser, rCam, palette);
    }

    private static void AddToContainer(On.PlayerGraphics.orig_AddToContainer orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContainer)
    {
        orig(self, sLeaser, rCam, newContainer);

        if (_initializingSprites) return;

        if (!(self.owner as Player)!.IsNomad(out _)) return;

        if (Wings.TryGetValue(self.player, out var wings))
            wings.AddToContainer(sLeaser, rCam, rCam.ReturnFContainer("Midground"));

        if (Whiskers.TryGetValue(self.player, out var whiskers))
            whiskers.AddToContainer(sLeaser, rCam, rCam.ReturnFContainer("Midground"));
    }

    /////////////////////////////////////
    // Wing splay
    /////////////////////////////////////

    private static bool SlugcatHandOnEngageInMovement(On.SlugcatHand.orig_EngageInMovement orig, SlugcatHand self)
    {
        Player player = (self.owner.owner as Player)!;

        if (!player.IsNomad(out var playerData)) return orig(self);

        // Checks (Don't do wing splay if any of these are true)

        if (playerData.GlideSpeed <= 0) return orig(self);

        if (player.animation == Player.AnimationIndex.ClimbOnBeam ||
            player.animation == Player.AnimationIndex.HangFromBeam ||
            player.bodyMode == Player.BodyModeIndex.Crawl ||
            player.bodyMode == Player.BodyModeIndex.CorridorClimb || // The player shouldn't spread their wings during these animations
            player.bodyMode == Player.BodyModeIndex.WallClimb ||

            player.animation == Player.AnimationIndex.RocketJump &&
            player.bodyChunks[1].lastPos.y - player.bodyChunks[1].pos.y < -1) return orig(self); // Without this the Nomad will immediatly spread their wings after pouncing, which annoys me

        if (Mathf.Abs(player.bodyChunks[1].lastPos.x - player.bodyChunks[1].pos.x) > 0.4f &&
            player.bodyMode == Player.BodyModeIndex.Stand) return orig(self); // Wing splay while walking looks funky, so don't

        // Try to make the hand as disabled as possible

        self.mode = Limb.Mode.Dangle;
        self.huntSpeed = 0f;
        self.quickness = 0f;
        self.retractCounter = 1;
        self.reachingForObject = false;
        self.retract = false;
        self.vel = Vector2.zero;

        // Hand position calculation and assignment

        // Calculation
        var playerOrientation = player.bodyChunks[0].pos - player.bodyChunks[1].pos; // A line from the top, to bottom of the player

        Vector2 targetPos =
            (player.bodyMode == Player.BodyModeIndex.Stand ? 50 : 52) * // The vector's amplitude
            (self.limbNumber - 0.5f) * // Differentiate left and right hands
            new Vector2(playerOrientation.y, -playerOrientation.x).normalized; // The player's orientation rotated 90 degrees

        targetPos += (player.bodyMode == Player.BodyModeIndex.Stand ?
            -5f : -1f) * playerOrientation.normalized; // Move downward relative to player orientation

        // Assignment
        self.relativeHuntPos = targetPos;
        self.pos = Vector2.Lerp(self.pos, player.firstChunk.pos + targetPos, 0.5f);

        return orig(self);
    }
}