using System;
using BepInEx;
using UnityEngine;
using ImprovedInput;
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

namespace SlugTemplate
{
    class HelperFuncs
    {
        public static bool CanGlide(Player player)
        {
            bool output = false;

            if (
                player.bodyMode != Player.BodyModeIndex.Crawl &&
                player.bodyMode != Player.BodyModeIndex.WallClimb &&
                player.bodyMode != Player.BodyModeIndex.CorridorClimb &&
                player.bodyMode != Player.BodyModeIndex.ClimbingOnBeam &&
                player.bodyMode != Player.BodyModeIndex.ClimbIntoShortCut &&
                player.bodyMode != Player.BodyModeIndex.ZeroG &&
                player.bodyMode != Player.BodyModeIndex.Swimming
            )
                output = true;

            return output;
        }
    }
}
