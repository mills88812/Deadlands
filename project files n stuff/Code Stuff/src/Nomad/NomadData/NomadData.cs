namespace Deadlands;

public class NomadData
{
    public DynamicSoundLoop windSound;

    public readonly bool IsNomad;

    public WeakReference<Player> PlayerRef;

    /// <summary>
    /// Represents the current speed nomad is gliding, ranges from 0 to 1 <br/>
    /// Will be 0.01 if the player is doing wing splay
    /// </summary>
    public float GlideSpeed = 0;

    /// <summary>
    /// The nomad's <b>y</b> coordinate from the start of their glide. </summary>
    /// <remarks>
    /// Used to calculate how far the player has fallen since the start of the glide
    /// </remarks>
    public float StartHeight = 0;

    /// <summary>
    /// Adds a decay effect to the <see cref="Player.superLaunchJump"/> variable (to detect if the player recently super jumped)
    /// </summary>
    public int SuperJumpDecay = 0;

    public NomadData(Player player)
    {
        IsNomad = player.slugcatStats.name == DeadlandsEnums.Nomad;
        PlayerRef = new WeakReference<Player>(player);

        if (!IsNomad) return;

        windSound = new ChunkDynamicSoundLoop(player.bodyChunks[0])
        {
            sound = DeadlandsEnums.Sound.wind,
            Pitch = 1f,
            Volume = 1f
        };
    }
}