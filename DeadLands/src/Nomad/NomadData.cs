using Deadlands.Enums;

namespace Deadlands.Nomad;

internal class NomadData
{
    public readonly DynamicSoundLoop windSound;

    public float Gliding = 0;
    
    
    public NomadData(Player player)
    {
        windSound = new ChunkDynamicSoundLoop(player.bodyChunks[1]);
        windSound.sound = DeadlandsEnums.Wind;
    }
}