using System;
using BepInEx;
using UnityEngine;
//using ImprovedInput;
using SlugBase.Features;
using static SlugBase.Features.FeatureTypes;
using System.Xml.Schema;
using DressMySlugcat;
using IL;
using System.Linq;
using On;
using System.Collections.Generic;
using SlugBase;
using Fisobs.Core;

namespace Deadlands;

class HelperFuncs
{
    public static bool CanGlide(Player player)
    {
        bool output = false;

        if (
            player.canJump <= 0 &&
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
            !player.Stunned
        )
            output = true;

        return output;
    }
}

