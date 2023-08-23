using System;
using SlugBase;
using Deadlands.Enums;

namespace Deadlands.Nomad;

internal class NomadEX
{
    private readonly float _slideStaminaRecoveryBase;
    private float _slideSpeed;
    private readonly int _slideStaminaMaxBase;
    private bool _unlockedExtraStamina = false;

    public float SlideRecovery =>
        _unlockedExtraStamina ? _slideStaminaRecoveryBase * 1.2f : _slideStaminaRecoveryBase;

    public float MinimumSlideStamina =>
        SlideStaminaMax * 0.1f;

    public float SlideStamina;

    public int SlideStaminaMax =>
        _unlockedExtraStamina ? (int)(_slideStaminaMaxBase * 1.6f) : _slideStaminaMaxBase;
        
    public bool CanSlide => SlideStaminaMax > 0 && _slideSpeed > 0;

    public int slideStaminaRecoveryCooldown;
    public int slideDuration;
    public int timeSinceLastSlide;
    public int preventSlide;
    public int preventGrabs;


    public readonly bool Nomad;
    public readonly bool isNomad;
    public bool isSliding;
    public bool UnlockedVerticalFlight = false;

        
    public SlugBaseCharacter Character;
    public WeakReference<Player> NomadRef;

    public DynamicSoundLoop windSound;

    public NomadEX(Player player)
    {
        isNomad =
            Plugin.SlideStamina.TryGet(player, out _slideStaminaMaxBase) &&
            Plugin.SlideRecovery.TryGet(player, out _slideStaminaRecoveryBase) &&
            Plugin.SlideSpeed.TryGet(player, out _slideSpeed) &&
            Plugin.Nomad.TryGet(player, out Nomad);

        NomadRef = new WeakReference<Player>(player);

        /*if (ExtEnumBase.TryParse(typeof(SlugcatStats.Name), "Nomad", true, out var extEnum))
        {
            Name = extEnum as SlugcatStats.Name;
        }*/

        windSound = new ChunkDynamicSoundLoop(player.bodyChunks[0]);
        windSound.sound = DeadlandsEnums.wind;
        windSound.Pitch = 1f;
        windSound.Volume = 1f;
        SlideStamina = SlideStaminaMax;
        timeSinceLastSlide = 200;
    }

    public void StopSliding()
    {
        slideDuration = 0;
        timeSinceLastSlide = 0;
        isSliding = false;
    }

    public void InitiateSlide()
    {
        if (!NomadRef.TryGetTarget(out var player))
        {
            return;
        }

        player.bodyMode = Player.BodyModeIndex.Default;
        player.animation = Player.AnimationIndex.None;
        player.wantToJump = 0;
        slideDuration = 0;
        timeSinceLastSlide = 0;
        isSliding = true;
    }

    public bool CanSustainFlight
    {
        get
        {
            if (!NomadRef.TryGetTarget(out var player))
            {
                return false;
            }

            return SlideStamina > 0 && preventSlide == 0 && player.canJump <= 0 &&
                   player.bodyMode != Player.BodyModeIndex.Crawl &&
                   player.bodyMode != Player.BodyModeIndex.CorridorClimb &&
                   player.bodyMode != Player.BodyModeIndex.ClimbIntoShortCut &&
                   player.animation != Player.AnimationIndex.HangFromBeam &&
                   player.animation != Player.AnimationIndex.ClimbOnBeam &&
                   player.bodyMode != Player.BodyModeIndex.WallClimb &&
                   player.bodyMode != Player.BodyModeIndex.Swimming && player.Consious && !player.Stunned &&
                   player.animation != Player.AnimationIndex.AntlerClimb &&
                   player.animation != Player.AnimationIndex.VineGrab &&
                   player.animation != Player.AnimationIndex.ZeroGPoleGrab;
        }
    }
}