using SlugBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Deadlands
{
    internal class NomadEX
    {
        public float SlideRecovery => UnlockedExtraStamina ? SlideStaminaRecoveryBase * 1.2f : SlideStaminaRecoveryBase;
        public float MinimumSlideStamina => SlideStaminaMax * 0.1f;

        public readonly float SlideStaminaRecoveryBase;
        public float SlideStamina;
        public float SlideSpeed;

        public int SlideStaminaMax => UnlockedExtraStamina ? (int)(SlideStaminaMaxBase * 1.6f) : SlideStaminaMaxBase;

        public readonly int SlideStaminaMaxBase;
        public int slideStaminaRecoveryCooldown;
        public int slideDuration;
        public int timeSinceLastSlide;
        public int preventSlide;
        public int preventGrabs;

        public bool CanSlide => SlideStaminaMax > 0 && SlideSpeed > 0;

        public readonly bool Nomad;
        public readonly bool isNomad;
        public bool isSliding;
        public bool UnlockedExtraStamina = false;
        public bool UnlockedVerticalFlight = false;

        public readonly SlugcatStats.Name Name;
        public SlugBaseCharacter Character;
        public WeakReference<Player> NomadRef;

        public DynamicSoundLoop windSound;

        public NomadEX(Player player)
        {
            isNomad =
                NomadPlugin.slideStamina.TryGet(player, out SlideStaminaMaxBase) &&
                NomadPlugin.SlideRecovery.TryGet(player, out SlideStaminaRecoveryBase) &&
                NomadPlugin.SlideSpeed.TryGet(player, out SlideSpeed) &&
                NomadPlugin.Nomad.TryGet(player, out Nomad);

            NomadRef = new WeakReference<Player>(player);

            if (ExtEnumBase.TryParse(typeof(SlugcatStats.Name), "Nomad", true, out var extEnum))
            {
                Name = extEnum as SlugcatStats.Name;
            }

            windSound = new ChunkDynamicSoundLoop(player.bodyChunks[0]);
            windSound.sound = NomadEnums.wind;
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

        public bool CanSustainFlight()
        {
            if (!NomadRef.TryGetTarget(out var player))
            {
                return false;
            }
            return SlideStamina > 0 && preventSlide == 0 && player.canJump <= 0 && player.bodyMode != Player.BodyModeIndex.Crawl && player.bodyMode != Player.BodyModeIndex.CorridorClimb && player.bodyMode != Player.BodyModeIndex.ClimbIntoShortCut && player.animation != Player.AnimationIndex.HangFromBeam && player.animation != Player.AnimationIndex.ClimbOnBeam && player.bodyMode != Player.BodyModeIndex.WallClimb && player.bodyMode != Player.BodyModeIndex.Swimming && player.Consious && !player.Stunned && player.animation != Player.AnimationIndex.AntlerClimb && player.animation != Player.AnimationIndex.VineGrab && player.animation != Player.AnimationIndex.ZeroGPoleGrab;
        }
    }
}