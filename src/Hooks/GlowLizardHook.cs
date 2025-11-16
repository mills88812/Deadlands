using Mono.Cecil.Cil;
using MonoMod.Cil;
using RWCustom;
using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using System.Reflection;
using Color = UnityEngine.Color;
using Random = UnityEngine.Random;
using LizardCosmetics;
using Deadlands;
using System.Collections.Generic;
using System.Drawing;
using Deadlands.Creatures.Glow;



namespace Deadlands.Hooks
{
    internal class GlowLizardHook
    {

         public static readonly ConditionalWeakTable<Lizard, GlowModule> GlowModule = new();

        public static void Apply()
        {
            On.Lizard.ctor += OnGlowctor;
            On.LizardVoice.GetMyVoiceTrigger += OnGlowVoice;
            On.LizardTongue.ctor += OnTongeglow;
            On.LizardLimb.ctor += OnGlowLimb;

            // On.LizardAI.ctor += OnIguanaAIctor;
            On.LizardAI.Update += OnGlowAIUpdate;
            On.LizardGraphics.ctor += OnGlowcolorctor;
            On.LizardGraphics.Update += OnUpdateGraphics;
            On.LizardGraphics.DrawSprites += OnwhiteGlow;
            On.LizardGraphics.ColorBody += OnGlowcolorBody;
            //  On.LizardGraphics.BodyColor += OnIguanaBodycolor;
            // On.LizardGraphics.DynamicBodyColor += IguanaLizardBodyColors3;
            //  On.LizardGraphics.ApplyPalette += OnIguanacolorBody;

            // On.LizardGraphics.InitiateSprites += OnGlowInitiateSprites;
            IL.OverseerAbstractAI.HowInterestingIsCreature += ILOverseeGlow;

            
        }

        
        private static void OnGlowctor(On.Lizard.orig_ctor orig, Lizard self, AbstractCreature abstractCreature, World world)
        {
            orig(self, abstractCreature, world);
            if (self.Template.type == DLCreature.GlowLizard)
            {

                var state = Random.state;
                Random.InitState(abstractCreature.ID.RandomSeed);

                self.tongue = new LizardTongue(self);
                self.effectColor = Custom.HSL2RGB(Custom.WrappedRandomVariation(.06f, .03f, .66f), 1f, Custom.ClampedRandomVariation(0.6f, .05f, 0.9f));

                Random.state = state;

                GlowModule.Add(self, new GlowModule(self));

            }
        }

        private static SoundID OnGlowVoice (On.LizardVoice.orig_GetMyVoiceTrigger orig, LizardVoice self)
        {
            SoundID res = orig(self);
            if (self.lizard is Lizard a && a.Template.type == DLCreature.GlowLizard)
            {
                var array = new[] { "A", "B", "C", "D", "E" };
                var list = new List<SoundID>();
                for (int i = 0; i<array.Length; i++)
                {
                    var soundID = SoundID.None;
                    var text2 = "Lizard_Voice_Blue_" + array[i];
                    if (SoundID.values.entries.Contains(text2))
                        soundID = new (text2);
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
        private static void OnTongeglow(On.LizardTongue.orig_ctor orig, LizardTongue self, Lizard lizard)
        {

            orig(self, lizard);
            if (lizard.Template.type == DLCreature.GlowLizard)
            {
                self.range = 180f;
                GlowLizardHook.s_elasticRange.SetValue(self, 0.8f);
                GlowLizardHook.s_totR.SetValue(self, self.range * 1.1f);
                self.lashOutSpeed = 30f;
                self.reelInSpeed = 0.002f;
                self.chunkDrag = 0.1f;
                self.terrainDrag = 0.05f;
                self.dragElasticity = 0.02f;
                self.emptyElasticity = 0.003f;
                self.involuntaryReleaseChance = 0.006f;
                self.voluntaryReleaseChance = 0.01f;
                self.baseDragOnly = true;
                self.attachesBackgroundWalls = false;
                self.attachTerrainChance = 0.3f;
                self.pullAtChunkRatio = 0.05f;
                self.detatchMinDistanceTerrain = 60f;
                self.totRExtraLimit = 80f;
            }
        }
        private static void OnGlowLimb(On.LizardLimb.orig_ctor orig, LizardLimb self, GraphicsModule owner, BodyChunk connectionChunk, int num, float rad, float sfFric, float aFric, float huntSpeed, float quickness, LizardLimb otherLimbInPair)
        {
            if (owner is LizardGraphics iguana && iguana.lizard.Template.type == DLCreature.GlowLizard)
            {
                self.grabSound = SoundID.Lizard_BlueWhite_Foot_Grab;
                self.releaseSeound = SoundID.Lizard_BlueWhite_Foot_Release;

            };


            orig(self, owner, connectionChunk, num, rad, sfFric, aFric, huntSpeed, quickness, otherLimbInPair);
        }
        private static void OnGlowAIUpdate(On.LizardAI.orig_Update orig, global::LizardAI self)
        {
            orig(self);

           
            if (self.lizard.Template.type == DLCreature.GlowLizard)
            {
                self.noiseTracker.hearingSkill = Custom.LerpMap(self.runSpeed, 0f, 0.7f, 1.8f, 0.7f);
            }

        }
        private static void OnGlowcolorctor(On.LizardGraphics.orig_ctor orig, LizardGraphics self, PhysicalObject ow)
        {
            orig(self, ow);

            Random.State state = Random.state;
            Random.InitState(self.lizard.abstractCreature.ID.RandomSeed);
            var num = self.startOfExtraSprites + self.extraSprites;
            int num2 = 0;
            if (self.lizard.Template.type == DLCreature.GlowLizard)
            {
            if (Random.value < 0.97f)
            {
                num = self.AddCosmetic(num, new glowcircul(self, num));
                num2++;
            }
            if (Random.value < 1f)
            {
                num = self.AddCosmetic(num, new TailTipGlow(self, num));
                num2++;
            }
            if (Random.value < 0.1f)
            {
                num = self.AddCosmetic(num, new BumpHawk(self, num));
                num2++;
            }

                Random.state = state;
            }

        }
        private static void OnUpdateGraphics(On.LizardGraphics.orig_Update orig, LizardGraphics self)
        {
            orig(self);


           
            if (self.lizard.Template.type == DLCreature.GlowLizard)
            {
                GlowModule.TryGetValue(self.lizard, out var data);
                if (data != null)
                {
                // Debug.Log("I have glow data");
                data.UpdateGraphics(self);
                }
            }

        }
        private static void OnwhiteGlow(On.LizardGraphics.orig_DrawSprites orig, LizardGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            orig(self, sLeaser, rCam, timeStacker, camPos);

            if (self.lizard.Template.type == DLCreature.GlowLizard)
            {
                sLeaser.sprites[self.SpriteHeadStart + 1].color = new Color(1f, 1f, 1f);
                sLeaser.sprites[self.SpriteHeadStart + 2].color = new Color(1f, 1f, 1f);
                sLeaser.sprites[self.SpriteHeadStart + 4].color = new Color(1f, 1f, 1f);
                
            }
            
        }
        
        
        


        private static void OnGlowcolorBody(On.LizardGraphics.orig_ColorBody orig, LizardGraphics self, RoomCamera.SpriteLeaser sLeaser, Color col)
        {

            //.21f, .53f, .9f
            orig(self, sLeaser, col);


            if (self.lizard.Template.type == DLCreature.GlowLizard)
            {


                for (int j = self.SpriteLimbsStart; j < self.SpriteLimbsEnd; j++)
                {
                    sLeaser.sprites[j].color = self.effectColor;
                }

            }

        }
        




       
        private static void ILOverseeGlow(ILContext il)
        {
            ILCursor c = new(il);
            ILLabel? label = null;
            if (c.TryGotoNext(
                x => x.MatchLdarg(1),
                x => x.MatchLdfld<AbstractCreature>("creatureTemplate"),
                x => x.MatchLdfld<CreatureTemplate>("type"),
                x => x.MatchLdsfld<CreatureTemplate.Type>("BlueLizard"),
                x => x.MatchCall(out _),
                x => x.MatchBrtrue(out label))
            && label != null)
            {
                c.Emit(OpCodes.Ldarg_1);
                c.EmitDelegate((AbstractCreature testCrit) => testCrit.creatureTemplate.type == DLCreature.GlowLizard);
                c.Emit(OpCodes.Brtrue, label);
            }
            else
                Plugin.Logger.LogFatal("Couldn't ILHook OverseerAbstractAI.HowInteresting Is Glow Lizard!");

        }
        internal static void Dispose()
        {
            GlowLizardHook.s_elasticRange = null;
            GlowLizardHook.s_totR = null;
        }

        private static FieldInfo s_totR = typeof(global::LizardTongue).GetField("totR", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        private static FieldInfo s_elasticRange = typeof(LizardTongue).GetField("elasticRange", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
    }
}
