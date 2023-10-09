using System.Linq;
using UnityEngine;

namespace Deadlands.Nomad;

internal static class NomadGliding
{
    private const float MinGlideForce = 0.075f;
    private const float MaxGlideForce = 0.25f;

    public static void OnInit()
    {
        On.Player.UpdateMSC += Player_UpdateMSC;
    }

    
    private static void Player_UpdateMSC(On.Player.orig_UpdateMSC orig, Player self)
    {
        orig(self);

        //Custom Nomad Data
        if (!Nomad.NomadData.TryGetValue(self, out var nomadData)) return;
        
        BodyChunk rootChunk = self.bodyChunks[1];
        
        do {
            nomadData.Gliding = 0;
            
            if (!self.input[0].jmp) break;
            
            if (self.animation != Player.AnimationIndex.None &&
                self.animation != Player.AnimationIndex.RocketJump) break; // Glide should only take effect during these two animations
            
            if (self.grasps.Any(g => g?.grabbed is Creature)) break;
            
            nomadData.Gliding = 0.01f; // Can do wing splay anytime
            
            if (self.bodyMode != Player.BodyModeIndex.Default || // Glide should only take effect when airborne
                rootChunk.lastPos.y - rootChunk.pos.y < -1) break; // Glide should only take effect when falling
            
            nomadData.Gliding = Mathf.Clamp((rootChunk.lastPos - rootChunk.pos).Abs().magnitude * 0.1f, 0.01f, 1); // Can do wing splay anytime
            
            GlidePhysics(self);
        }
        while (false);
        
        float vol = nomadData.windSound.Volume;

        nomadData.windSound.Volume = nomadData.Gliding > 0 &&
                                     self.bodyMode == Player.BodyModeIndex.Default &&
                                     rootChunk.lastPos.y - rootChunk.pos.y > -1 ?
            Mathf.Min(vol + 0.3f, 0.6f * nomadData.Gliding) :
            Mathf.Max(vol - 0.5f, 0f);
        
        nomadData.windSound.Update();
    }
    
    private static void GlidePhysics(Player player)
    {
        var rootChunk = player.bodyChunks[1];
        
        rootChunk.vel.y = Mathf.Lerp(rootChunk.vel.y, 0,
            Mathf.Lerp(MinGlideForce, MaxGlideForce, (Mathf.Abs(rootChunk.vel.x) - 2.5f) * 0.2f));
            
        rootChunk.vel.x -= 2 * player.input[0].x;
        player.firstChunk.vel.x += 2 * player.input[0].x;
    }
}
