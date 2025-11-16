using Deadlands.Creatures.Iguana;
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
using Mono.Cecil;
using Deadlands.Features.Opal;



namespace Deadlands.Hooks
{
    internal class IguanaHook
    {

        public static readonly ConditionalWeakTable<Lizard, IguanaModule> IguanaModule = new();
        
        public static void Apply()
        {
            On.Lizard.ctor += OnIguanactor;

            On.LizardVoice.GetMyVoiceTrigger += OnIguanaVoice;

            On.Lizard.Violence += OnIguanaViolence;

           // On.LizardGraphics.DrawSprites += OnIguanaDrawSprites;

            On.LizardLimb.ctor += OnIguanaLimb;

            // On.LizardAI.ctor += OnIguanaAIctor;

            On.LizardGraphics.ctor += OnIguanacolorctor;

            //  On.LizardGraphics.InitiateSprites += OnInitiateSprites;

            // On.LizardGraphics.Update += OnUpdateGraphics;

            On.Lizard.Collide += OnFood;

            On.LizardGraphics.BodyColor += OnIguanaBodycolor;
          //  On.LizardGraphics.ColorBody += OnIguanacolor;
            // On.LizardGraphics.DynamicBodyColor += IguanaLizardBodyColors3;
            On.LizardGraphics.ApplyPalette += OnIguanacolorBody;
            On.LizardGraphics.DrawSprites += Onhead;
            IL.OverseerAbstractAI.HowInterestingIsCreature += ILOverseeIguana;
            LizardCosmetics.Init();
        }

       
        private static void OnIguanactor(On.Lizard.orig_ctor orig, Lizard self, AbstractCreature abstractCreature, World world)
        {
            orig(self, abstractCreature, world);
            if (self.Template.type == DLCreature.Iguana)
            {

                var state = Random.state;
                Random.InitState(abstractCreature.ID.RandomSeed);
                self.effectColor = Custom.HSL2RGB(Custom.WrappedRandomVariation(.19f, .12f, .4f), 0.6f, Custom.ClampedRandomVariation(0.34f, .12f, .32f));
                Random.state = state;
                IguanaModule.Add(self, new IguanaModule(self));
            }



        }

        private static SoundID OnIguanaVoice(On.LizardVoice.orig_GetMyVoiceTrigger orig, LizardVoice self)
        {
            SoundID res = orig(self);
            if (self.lizard is Lizard a && a.Template.type == DLCreature.Iguana)
            {
                var array = new[] { "A", "B", "C", "D", "E" };
                var list = new List<SoundID>();
                for (int i = 0; i < array.Length; i++)
                {
                    var soundID = SoundID.None;
                    var text2 = "Lizard_Voice_White_" + array[i];
                    if (SoundID.values.entries.Contains(text2))
                        soundID = new(text2);
                    if (soundID != SoundID.None && soundID.Index != -1 && a.abstractCreature.world.game.soundLoader.workingTriggers[soundID.Index])
                        list.Add(soundID);
                }
                if (list.Count == 0)
                    res = SoundID.None;
                else
                    res = list[Random.Range(0, list.Count)];
            }
            return res;
        }
        private static void OnIguanaLimb(On.LizardLimb.orig_ctor orig, LizardLimb self, GraphicsModule owner, BodyChunk connectionChunk, int num, float rad, float sfFric, float aFric, float huntSpeed, float quickness, LizardLimb otherLimbInPair)
        {
            if (owner is LizardGraphics iguana && iguana.lizard.Template.type == DLCreature.Iguana)
            {
                self.grabSound = SoundID.Lizard_PinkYellowRed_Foot_Grab;
                self.releaseSeound = SoundID.Lizard_PinkYellowRed_Foot_Release;

            };


            orig(self, owner, connectionChunk, num, rad, sfFric, aFric, huntSpeed, quickness, otherLimbInPair);
        }

        private static void OnIguanaViolence(On.Lizard.orig_Violence orig, global::Lizard self, global::BodyChunk source, Vector2? directionAndMomentum, global::BodyChunk hitChunk, global::PhysicalObject.Appendage.Pos onAppendagePos, global::Creature.DamageType type, float damage, float stunBonus)
        {
            if (self.Template.type == DLCreature.Iguana)
            {
                if (hitChunk.index == 3)
                {
                    DrodTail();
                };
            }

            orig(self, source, directionAndMomentum, hitChunk, onAppendagePos, type, damage, stunBonus);
        }
        private static void OnIguanaDrawSprites(On.LizardGraphics.orig_DrawSprites orig, global::LizardGraphics self, global::RoomCamera.SpriteLeaser sLeaser, global::RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            if (self.lizard.Template.type == DLCreature.Iguana)
            {
                
                    sLeaser.sprites[self.SpriteTail].isVisible = false;
            }

            orig(self, sLeaser, rCam, timeStacker, camPos);
        }
        private static void DrodTail()
        {
            

        }
        private static void OnIguanacolorctor(On.LizardGraphics.orig_ctor orig, LizardGraphics self, PhysicalObject ow)
        {
            orig(self, ow);
            /*if (self.lizard != null && IguanaModule.TryGetValue(self.lizard, out IguanaModule data))
            {*/
                if (self.lizard.Template.type == DLCreature.Iguana)
                {

                    Random.State state = Random.state;
                    Random.InitState(self.lizard.abstractCreature.ID.RandomSeed);

                    int num = self.startOfExtraSprites + self.extraSprites;
                    int num2 = 0;

                    if (Random.value < 0.4f)
                    {
                        num = self.AddCosmetic(num, new SpineSpikes(self, num));
                        num2++;
                    }
                    self.ivarBodyColor = new HSLColor(Random.Range(.34f, 0.34f), Random.Range(0.23f, 0.6f), .23f).rgb;

                    Random.state = state;
                }
          //  }

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
        private static void OnFood(On.Lizard.orig_Collide orig, global::Lizard self, global::PhysicalObject otherObject, int myChunk, int otherChunk)
        {

            //.21f, .53f, .9f
            orig(self, otherObject, myChunk, otherChunk);

            if (self.Template.type == DLCreature.Iguana)
            {
                if (otherObject is  DangleFruit || otherObject is  SwollenWaterNut || otherObject is Opal || otherObject is SlimeMold)
                {
                    
                


                IguanaModule.TryGetValue(self, out var data);
                if (data != null)
                {

                    //Debug.Log("I have data");
                    data.Collide(orig, self, otherObject, myChunk, otherChunk);
                }
                }
            }


        }

        private static Color OnIguanaBodycolor(On.LizardGraphics.orig_BodyColor orig, LizardGraphics self, float f)
        {

            //.21f, .53f, .9f
            orig(self, f);
            float value = Mathf.InverseLerp(self.bodyLength / self.BodyAndTailLength, 1f, f);
            float f2 = Mathf.Clamp(Mathf.InverseLerp(self.lizard.lizardParams.tailColorationStart, 0.95f, value), 0f, 1f);
            f2 = Mathf.Pow(f2, self.lizard.lizardParams.tailColorationExponent) ;
            if (self.lizard.Template.type == DLCreature.Iguana && (f < self.bodyLength / self.BodyAndTailLength))
            {

                return Custom.HSL2RGB(Custom.WrappedRandomVariation(.15f, .20f, .4f), 0.6f, Custom.ClampedRandomVariation(0.34f, .12f, .32f));
            }
            if (self.lizard.Template.type == DLCreature.Iguana)
            {

                return Color.Lerp(new HSLColor(Random.Range(.15f, 0.34f), Random.Range(0.23f, 0.6f), .23f).rgb, Custom.HSL2RGB(Custom.WrappedRandomVariation(.19f, .12f, .4f), 0.6f, Custom.ClampedRandomVariation(0.34f, .12f, .32f)), f2);
                
            }


            return orig(self, f);
        }

        private static void OnIguanacolor(On.LizardGraphics.orig_ColorBody orig, LizardGraphics self, RoomCamera.SpriteLeaser sLeaser, Color col)
        {

            //.21f, .53f, .9f
            orig(self, sLeaser, col);

            if (self.lizard.Template.type == DLCreature.Iguana)
            {
                


                for (int i = self.SpriteBodyCirclesStart; i < self.SpriteBodyCirclesEnd; i++)
                {
                    sLeaser.sprites[1].color = Custom.HSL2RGB(Custom.WrappedRandomVariation(.15f, .20f, .4f), 0.6f, Custom.ClampedRandomVariation(0.34f, .12f, .32f)); 
                    sLeaser.sprites[2].color = Custom.HSL2RGB(Custom.WrappedRandomVariation(.15f, .20f, .4f), 0.6f, Custom.ClampedRandomVariation(0.34f, .12f, .32f));
                    sLeaser.sprites[3].color = Custom.HSL2RGB(Custom.WrappedRandomVariation(.15f, .20f, .4f), 0.6f, Custom.ClampedRandomVariation(0.34f, .12f, .32f));
                }
                
            }

        }

        private static void OnIguanacolorBody(On.LizardGraphics.orig_ApplyPalette orig, LizardGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {

            //.21f, .53f, .9f
            orig(self, sLeaser, rCam, palette);
            if (self.lizard.Template.type == DLCreature.Iguana)
            {
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

        private static void Onhead(On.LizardGraphics.orig_DrawSprites orig, LizardGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            orig(self, sLeaser, rCam, timeStacker, camPos);

            if (self.lizard.Template.type == DLCreature.Iguana)
            {
                float headAngleNumber = Mathf.Lerp(self.lastHeadDepthRotation, self.headDepthRotation, timeStacker);
                int headAngle = 3 - (int)(Mathf.Abs(headAngleNumber) * 3.9f);
                Custom.PerpendicularVector(Vector2.Lerp(self.drawPositions[0, 1], self.drawPositions[0, 0], timeStacker) - Vector2.Lerp(self.head.lastPos, self.head.pos, timeStacker));
                sLeaser.sprites[self.SpriteHeadStart].element = Futile.atlasManager.GetElementWithName("IguanaJaw" + 0 + "." + headAngle);
                sLeaser.sprites[self.SpriteHeadStart + 1].element = Futile.atlasManager.GetElementWithName("IguanaLowerteeths" + 0 + "." + headAngle);
                sLeaser.sprites[self.SpriteHeadStart + 2].element = Futile.atlasManager.GetElementWithName("IguanaUpperteeths" + 0 + "." + headAngle);
                sLeaser.sprites[self.SpriteHeadStart + 3].element = Futile.atlasManager.GetElementWithName("IguanaHead" + 0 + "." + headAngle);
                sLeaser.sprites[self.SpriteHeadStart + 4].element = Futile.atlasManager.GetElementWithName("IguanaEyes" + 0 + "." + headAngle);



            }

        }
        private static Color IguanaLizardBodyColors3(On.LizardGraphics.orig_DynamicBodyColor orig, LizardGraphics self, float f)
        {
            orig(self, f);
            if (self.lizard.Template.type == DLCreature.Iguana)
            {
                return Color.Lerp(self.lizard.effectColor, new Color(.21f, .53f, .09f), 0.5f);
            }
            return orig(self, f);
        }
        private static void OnIguanaAIctor(On.LizardAI.orig_ctor orig, LizardAI self, AbstractCreature creature, World world)
        {
            orig(self, creature, world);
            if (self.lizard.Template.type == DLCreature.Iguana)
            {
                self.pathFinder.accessibilityStepsPerFrame = 20;
                self.preyTracker.sureToGetPreyDistance = 120;
            }
        }

        private static void ILOverseeIguana(ILContext il)
        {
            ILCursor c = new(il);
            ILLabel? label = null;
            if (c.TryGotoNext(
                x => x.MatchLdarg(1),
                x => x.MatchLdfld<AbstractCreature>("creatureTemplate"),
                x => x.MatchLdfld<CreatureTemplate>("type"),
                x => x.MatchLdsfld<CreatureTemplate.Type>("PinkLizard"),
                x => x.MatchCall(out _),
                x => x.MatchBrtrue(out label))
            && label != null)
            {
                c.Emit(OpCodes.Ldarg_1);
                c.EmitDelegate((AbstractCreature testCrit) => testCrit.creatureTemplate.type == DLCreature.Iguana);
                c.Emit(OpCodes.Brtrue, label);
            }
            else
                Plugin.Logger.LogFatal("Couldn't ILHook OverseerAbstractAI.HowInteresting Is Iguana Lizard!");

        }
    }

}
