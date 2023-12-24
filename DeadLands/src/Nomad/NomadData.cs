using Deadlands.Enums;

namespace Deadlands.Nomad;

internal class NomadData
{
    public readonly DynamicSoundLoop windSound;
    
    
    /// <summary>
    /// Represents the current speed nomad is gliding, ranges from 0 to 1
    /// Will be 0.01 if the player is doing wing splay
    /// </summary>
    public float GlideSpeed = 0;
    
    /// <summary>
    /// The nomad's y coordinate from the start of their glide
    /// Used to calculate how far the player has fallen since the start of the glide
    /// </summary>
    public float StartHeight = 0;
    
    /// <summary>
    /// Adds a decay effect to the Player.superLaunchJump variable (to detect if the player recently super jumped)
    /// </summary>
    public int SuperJumpDecay = 0;
    

    public NomadData(Player player)
    {
        windSound = new ChunkDynamicSoundLoop(player.bodyChunks[1]);
        windSound.sound = DeadlandsEnums.Wind;
    }
}