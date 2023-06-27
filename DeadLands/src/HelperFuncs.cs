namespace Deadlands;

static class HelperFuncs
{
    public static bool CanGlide(this Player player)
        => player.canJump <= 0 &&
           player.bodyMode != Player.BodyModeIndex.Crawl &&
           player.bodyMode != Player.BodyModeIndex.CorridorClimb &&
           player.bodyMode != Player.BodyModeIndex.ClimbIntoShortCut &&
           player.bodyMode != Player.BodyModeIndex.WallClimb &&
           player.bodyMode != Player.BodyModeIndex.Swimming &&
           player.animation != Player.AnimationIndex.HangFromBeam &&
           player.animation != Player.AnimationIndex.ClimbOnBeam &&
           player.animation != Player.AnimationIndex.AntlerClimb &&
           player.animation != Player.AnimationIndex.VineGrab &&
           player.animation != Player.AnimationIndex.ZeroGPoleGrab &&
           player.Consious &&
           !player.Stunned;
}