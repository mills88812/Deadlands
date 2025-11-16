using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using System.Collections.Generic;
using UnityEngine;
using RWCustom;
using System.Runtime.CompilerServices;
using Color = UnityEngine.Color;
using Random = UnityEngine.Random;
using LizardCosmetics;
using System.Drawing;



namespace Deadlands.Hooks
{
    internal class BrownLizardHook
    {



        public static void Apply()
        {
            On.Lizard.ctor += OnBrownctor;
          //  On.Lizard.Update += Onupdate;
            On.LizardLimb.ctor += OnBrownLimb;

            // On.LizardAI.ctor += OnIguanaAIctor;

            On.LizardGraphics.ctor += OnBrowncolorctor;

            //  On.LizardGraphics.InitiateSprites += OnInitiateSprites;

            // On.LizardGraphics.Update += OnUpdateGraphics;

            On.LizardGraphics.BodyColor += OnBrownBodycolor;
            // On.LizardGraphics.DynamicBodyColor += IguanaLizardBodyColors3;
            On.LizardGraphics.ApplyPalette += OnBrowncolorBody;
            IL.OverseerAbstractAI.HowInterestingIsCreature += ILOverseeIguana;
            
        }

        
        private static void OnBrownctor(On.Lizard.orig_ctor orig, Lizard self, AbstractCreature abstractCreature, World world)
        {
            orig(self, abstractCreature, world);
            if (self.Template.type == DLCreature.BrownLizard)
            {

                var state = Random.state;
                Random.InitState(abstractCreature.ID.RandomSeed);
                self.effectColor = Custom.HSL2RGB(Custom.WrappedRandomVariation(.09f, 0.02f, .03f), 1f, 0.18f);
                Random.state = state;

            }



        }
        private static void Onupdate(On.Lizard.orig_Update orig, Lizard self, bool eu)
        {
            orig(self, eu);
            
            if (!self.dead && self.stun < 35 && self.grabbedBy.Count > 0 && !self.safariControlled)
            {
                self.grabbedAttackCounter++;
                self.jawForcedShut = Mathf.Max(0f, self.jawForcedShut - 0.041666668f);
                self.JawOpen = self.JawOpen + 0.1f + 0.1f * self.LizardState.health;
                for (int i = 0; i < self.bodyChunks.Length; i++)
                {
                    self.bodyChunks[i].vel += Custom.RNV() * Random.value * 6f * self.LizardState.health;
                }
                if (self.Template.type == DLCreature.BrownLizard )
                {
                    self.DamageAttackClosestChunk(self.grabbedBy[0].grabber);
                }
            }

            self.Update(eu);
        }


        private static void OnBrownLimb(On.LizardLimb.orig_ctor orig, LizardLimb self, GraphicsModule owner, BodyChunk connectionChunk, int num, float rad, float sfFric, float aFric, float huntSpeed, float quickness, LizardLimb otherLimbInPair)
        {
            if (owner is LizardGraphics iguana && iguana.lizard.Template.type == DLCreature.BrownLizard)
            {
                self.grabSound = SoundID.Lizard_Green_Foot_Grab;
                self.releaseSeound = SoundID.Lizard_Green_Foot_Release;

            };


            orig(self, owner, connectionChunk, num, rad, sfFric, aFric, huntSpeed, quickness, otherLimbInPair);
        }
        private static void OnBrowncolorctor(On.LizardGraphics.orig_ctor orig, LizardGraphics self, PhysicalObject ow)
        {
            orig(self, ow);
            Random.State state = Random.state;
            Random.InitState(self.lizard.abstractCreature.ID.RandomSeed);
            var num = self.startOfExtraSprites + self.extraSprites;
            int num2 = 0;
            if (self.lizard.Template.type == DLCreature.BrownLizard)
            {



                

               /* if (Random.value < 0.6f)
                {
                    num = self.AddCosmetic(num, new BodyStripes(self, num));
                    num2++;
                }
                if (Random.value < 0.7f)
                {
                    num = self.AddCosmetic(num, new BodyStripes(self, num));
                    num2++;
                }
                if (Random.value < 0.8f)
                {
                    num = self.AddCosmetic(num, new BodyStripes(self, num));
                    num2++;
                }
                if (Random.value < 0.9f)
                {
                    num = self.AddCosmetic(num, new BodyStripes(self, num));
                    num2++;
                }*/
                for (int k = 0; k < self.lizard.lizardParams.tailSegments; k++)
                {
                    float num4 = Mathf.InverseLerp(0f, (float)(self.lizard.lizardParams.tailSegments - 1), (float)k);
                    self.tail[k].rad += Mathf.Sin(Mathf.Pow(num4, 0.7f) * 3.1415927f) * 2.5f;
                    self.tail[k].rad *= 1f - Mathf.Sin(Mathf.InverseLerp(0f, 0.4f, num4) * 3.1415927f) * 0.5f;
                }
                self.ivarBodyColor = new HSLColor(Random.Range(.09f, 0.09f), Random.Range(0.4f, 0.9f), .8f).rgb;
                if (Random.value < 0.3f)
                {
                    self.overrideHeadGraphic = 3;
                }
                else
                {
                    self.overrideHeadGraphic = 1;
                }
               // num = self.AddCosmetic(num, new BodyStripes(self, num));
                num = self.AddCosmetic(num, new SpikePorkipine(self, num));








                Random.state = state;
            }

        }
        private static void OnInitiateSprites(On.LizardGraphics.orig_InitiateSprites orig, LizardGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            try
            {
                orig(self, sLeaser, rCam);
            }
            catch (Exception e)
            {
                Debug.Log(e);
                for (int i = 0; i < sLeaser.sprites.Length; i++)
                {
                    if (sLeaser.sprites[i] != null && sLeaser.sprites[i].element != null)
                    {
                        Debug.Log(i + " | " + sLeaser.sprites[i].element.name);
                    }
                    else
                    {
                        Debug.Log(i + " | NULL!");
                    }
                }
            }
            Debug.Log(sLeaser.sprites.Length);
            if (self.lizard.Template.type == DLCreature.Iguana)
            {




                self.AddToContainer(sLeaser, rCam, null);
            }
            for (int i = 0; i < sLeaser.sprites.Length; i++)
            {
                Debug.Log(i + " | " + sLeaser.sprites[i].element.name);
            }
        }

        private static Color OnBrownBodycolor(On.LizardGraphics.orig_BodyColor orig, LizardGraphics self, float f)
        {

            
            orig(self, f);

           
            if (self.lizard.Template.type == DLCreature.BrownLizard)
            {

                return self.ivarBodyColor;
            }
            float value = Mathf.InverseLerp(self.bodyLength / self.BodyAndTailLength, 1f, f);
            float f2 = Mathf.Clamp(Mathf.InverseLerp(self.lizard.lizardParams.tailColorationStart, 0.95f, value), 0f, 1f);
            f2 = Mathf.Pow(f2, self.lizard.lizardParams.tailColorationExponent) * self.iVars.tailColor;
            if (self.lizard.Template.type == DLCreature.BrownLizard && (f < self.bodyLength / self.BodyAndTailLength || self.iVars.tailColor == 0f))
            {

                return Color.Lerp(self.ivarBodyColor, self.effectColor, f2);
            }

            return orig(self, f);
        }


        private static void OnBrowncolorBody(On.LizardGraphics.orig_ApplyPalette orig, LizardGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            //new HSLColor(UnityEngine.Random.Range(0.075f, 0.125f), UnityEngine.Random.Range(0.4f, 0.9f), num8).rgb
            
            orig(self, sLeaser, rCam, palette);
            if (self.lizard.Template.type == DLCreature.BrownLizard)
            {
                // self.ColorBody(sLeaser, Custom.HSL2RGB(Custom.WrappedRandomVariation(.19f, .12f, .4f), 0.6f, Custom.ClampedRandomVariation(0.34f, .12f, .32f)));
                self.ColorBody(sLeaser, self.ivarBodyColor);
                if (self.iVars.tailColor > 0f)
                {
                    for (int j = 0; j < (sLeaser.sprites[self.SpriteTail] as TriangleMesh).verticeColors.Length; j++)
                    {
                        float t = (float)(j / 2) * 2f / (float)((sLeaser.sprites[self.SpriteTail] as TriangleMesh).verticeColors.Length - 1);
                        (sLeaser.sprites[self.SpriteTail] as TriangleMesh).verticeColors[j] = self.BodyColor(Mathf.Lerp(self.bodyLength / self.BodyAndTailLength, 1f, t));
                    }
                }
            }



        }
        private static Color IguanaLizardBodyColors3(On.LizardGraphics.orig_DynamicBodyColor orig, LizardGraphics self, float f)
        {
            orig(self, f);
            if (self.lizard.Template.type == DLCreature.BrownLizard)
            {
                return Color.Lerp(self.lizard.effectColor, new Color(.21f, .53f, .09f), 0.5f);
            }
            return orig(self, f);
        }
        

        private static void ILOverseeIguana(ILContext il)
        {
            ILCursor c = new(il);
            ILLabel? label = null;
            if (c.TryGotoNext(
                x => x.MatchLdarg(1),
                x => x.MatchLdfld<AbstractCreature>("creatureTemplate"),
                x => x.MatchLdfld<CreatureTemplate>("type"),
                x => x.MatchLdsfld<CreatureTemplate.Type>("GreenLizard"),
                x => x.MatchCall(out _),
                x => x.MatchBrtrue(out label))
            && label != null)
            {
                c.Emit(OpCodes.Ldarg_1);
                c.EmitDelegate((AbstractCreature testCrit) => testCrit.creatureTemplate.type == DLCreature.BrownLizard);
                c.Emit(OpCodes.Brtrue, label);
            }
            else
                Plugin.Logger.LogFatal("Couldn't ILHook OverseerAbstractAI.HowInteresting Is Iguana Lizard!");

        }
    }
}
