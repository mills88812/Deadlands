using System;
using System.Drawing;
using System.Runtime.CompilerServices;
using Deadlands;
using IL;
using LizardCosmetics;
using MoreSlugcats;
using On;
using RWCustom;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Deadlands
{
    
    public class LizardCosmetics
    {
        
        public static void Init()
        {
            On.LizardCosmetics.BumpHawk.ctor += BumpHawkNegation;

            On.LizardCosmetics.TailTuft.ctor += TailTuftNegation;
        }

        
        public static void BumpHawkNegation(On.LizardCosmetics.BumpHawk.orig_ctor orig, BumpHawk BH, LizardGraphics liz, int startSprite)
        {
            orig(BH, liz, startSprite);
            if (liz.lizard.Template.type == DLCreature.Iguana)
            {
                BH.numberOfSprites = 0;
            }
            
        }

        
        public static void TailTuftNegation(On.LizardCosmetics.TailTuft.orig_ctor orig, TailTuft TT, LizardGraphics liz, int startSprite)
        {
            orig(TT, liz, startSprite);
            if (liz.lizard.Template.type == DLCreature.Iguana)
            {
                Array.Resize<LizardScale>(ref TT.scaleObjects, TT.scaleObjects.Length - TT.scalesPositions.Length);
                Array.Resize<Vector2>(ref TT.scalesPositions, TT.scalesPositions.Length - (TT.colored ? (TT.numberOfSprites / 2) : TT.numberOfSprites));
                TT.numberOfSprites = 0;
            }
            

        }

       

    }
}
