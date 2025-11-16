using Deadlands;
using Deadlands.Creatures.Buzzard;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RWCustom;
using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using Color = UnityEngine.Color;
using Random = UnityEngine.Random;

namespace Deadlands.Hooks
{
    internal class BuzzardHooks
    {
        public static readonly ConditionalWeakTable<Vulture, BuzzardModule> BuzzardModule = new();

        public static int TotalBuzzardSprites(VultureGraphics graphics)
        {
            if (!graphics.IsMiros)
            {
                return graphics.HeadSprite + (graphics.IsKing ? (6 + graphics.kngtskSprCount) : 5) + 1;
            }
            return 4 + graphics.LastBeakSprite();
        }

        public static void Apply()
        {

            // Vulture

            On.Vulture.ctor += OnVultureCtor;
            On.Vulture.Act += OnVultureAct;
            On.Vulture.AirBrake += OnAirBrake;

            IL.Vulture.ctor += ILVutureCtor;
            IL.Vulture.Violence += ILVultureViolence;

            // State

            On.Vulture.VultureState.ctor += OnVultureStateCtor;

            // AI

            On.VultureAI.ctor += OnVultureAICtor;

            // Graphics

            On.VultureGraphics.ctor += OnVultureGraphicsCtor;
            On.VultureGraphics.InitiateSprites += OnInitiateSprites;
            //On.VultureGraphics.DrawSprites += OnDrawSprites;
            On.VultureGraphics.Update += OnUpdateGraphics;
            //On.VultureGraphics.AddToContainer += OnAddToContainer;

            IL.VultureGraphics.ctor += ILVultureGraphicsCtor;
            IL.VultureGraphics.InitiateSprites += ILInitiateSprites;
            IL.VultureGraphics.ExitShadowMode += ILExitShadowMode;
            IL.VultureGraphics.DrawSprites += ILDrawSprites;
            //IL.VultureGraphics.InitiateSprites += ILInitiateSprites;

            // Vulture Tentacle
            On.VultureTentacle.ReleaseGrip += OnReleaseGrip;

            // Vulture Mask

            // PreyTracker

            IL.PreyTracker.TrackedPrey.Attractiveness += ILAttractiveness;

            
        }

        /// <summary>
        /// Buzzard Body colors, Vulture body colors are set when they exit shadow mode. (Which is every frame they're not in shadow mode)
        /// </summary>
        private static void ILExitShadowMode(ILContext il)
        {
            var c = new ILCursor(il);

            if (c.TryGotoNext(MoveType.After,
                    x => x.MatchLdarg(0),
                    x => x.MatchLdflda(typeof(VultureGraphics).GetField("palette")),
                    x => x.MatchLdfld(typeof(RoomPalette).GetField("blackColor")),
                    x => x.MatchStloc(0)))
            {
                c.Emit(OpCodes.Ldarg_0);
                //c.Emit(OpCodes.Ldloc_0);
                c.EmitDelegate<Func<VultureGraphics, Color>>((graphics) =>
                {
                    if (graphics.vulture.Template.type == DLCreature.Buzzard)
                    {
                        return Color.Lerp(graphics.palette.blackColor, new Color(0.3f, 0.19f, 0.1f), 0.86f - graphics.palette.darkness / 1.8f);
                    }
                    return graphics.palette.blackColor;
                });
                c.Emit(OpCodes.Stloc_0);
            }
            else
            {
                Plugin.Logger.LogDebug("ILExitShadowMode Failed!");
            }
        }

        private static void OnVultureAct(On.Vulture.orig_Act orig, Vulture self, bool eu)
        {
            orig(self, eu);
            if (self.Template.type == DLCreature.Buzzard)
            {
                BuzzardModule.TryGetValue(self, out var module);
                if (module != null)
                {
                    try
                    {
                        module.Act(self);
                    } catch(Exception ex)
                    {
                        Debug.LogError("Error running Act");
                        Debug.LogError(ex);
                    }
                }
            }
        }

        private static void OnUpdateGraphics(On.VultureGraphics.orig_Update orig, VultureGraphics self)
        {
            orig(self);
            if (self.vulture.Template.type == DLCreature.Buzzard)
            {
                BuzzardModule.TryGetValue(self.vulture, out var data);
                if (data != null)
                {
                    //Debug.Log("I have data");
                    data.UpdateGraphics(self);
                }
            }
        }

        /// <summary>
        /// We want to draw sprites around when KingTusk sprites are drawn so this ILHook will do just that.
        /// </summary>
        private static void ILDrawSprites(ILContext il)
        {
            var c = new ILCursor(il);

            if (c.TryGotoNext(MoveType.Before, // IL_0153: callvirt System.Void KingTusks::DrawSprites(VultureGraphics,RoomCamera/SpriteLeaser,RoomCamera,System.Single,UnityEngine.Vector2)
                    x => x.MatchLdarg(0),
                    x => x.MatchCall(typeof(VultureGraphics).GetMethod("get_IsMiros")),
                    x => x.MatchBrfalse(out _)))
            {
                c.Emit(OpCodes.Ldarg_0);
                c.Emit(OpCodes.Ldarg_1);
                c.Emit(OpCodes.Ldarg_2);
                c.Emit(OpCodes.Ldarg_3);
                c.Emit(OpCodes.Ldarg_S, (byte)4);
                c.EmitDelegate<Action<VultureGraphics, RoomCamera.SpriteLeaser, RoomCamera, float, Vector2>>((graphics, sLeaser, rCam, timeStacker, camPos) =>
                {
                    //Debug.Log(graphics + " " + sLeaser + " " + rCam + " " + timeStacker + " " + camPos);
                    if (graphics.vulture.Template.type == DLCreature.Buzzard)
                    {
                        BuzzardModule.TryGetValue(graphics.vulture, out var data);
                        if (data != null)
                        {
                            data.DrawSprites(graphics, sLeaser, rCam, timeStacker, camPos);
                        }
                    }
                });
            } else
            {
                Plugin.Logger.LogFatal("ILDrawSprites failed!");
            }
        }

        private static void OnDrawSprites(On.VultureGraphics.orig_DrawSprites orig, VultureGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            orig(self, sLeaser, rCam, timeStacker, camPos);
            // Post DrawSprites, Not currently in use
            /*
            if (self.vulture.Template.type == Type.Buzzard)
            {
                BuzzardModule.TryGetValue(self.vulture, out var data);
                if (data != null)
                {
                    data.DrawSprites(self, sLeaser, rCam, timeStacker, camPos);
                }
            }
            */
        }

        #region Vulture

        private static void ILVutureCtor(ILContext il)
        {
            var c = new ILCursor(il);

            ILLabel postCheck = null;

            if (c.TryGotoNext(MoveType.After, // Smaller body chunks
                x => x.MatchLdcR4(1.4f),
                x => x.MatchStloc(0)))
            {
                c.Emit(OpCodes.Ldarg_0); // Thank you forthbridge for this method, learned about it from pearl cat and used it throughout the project
                c.Emit(OpCodes.Ldloc_0);
                c.EmitDelegate<Func<Vulture, float, float>>((vulture, value) =>
                {
                    if (vulture.Template.type == DLCreature.Buzzard)
                    {
                        return 0.7f;
                    }
                    return value;
                });
                c.Emit(OpCodes.Stloc_0);
            }
            else
            {
                Plugin.Logger.LogFatal("ILVutureCtor failed!");
            }
        }
        /// <summary>
        /// Extra wing health since Buzzard wings can take a bigger beating.
        /// </summary>
        private static void OnVultureStateCtor(On.Vulture.VultureState.orig_ctor orig, Vulture.VultureState self, AbstractCreature creature)
        {
            orig(self, creature);
            if (creature.creatureTemplate.type == DLCreature.Buzzard)
            {
                for (int i = 0; i < self.wingHealth.Length; i++)
                {
                    self.wingHealth[i] = 2f;
                }
            }
        }

        /// <summary>
        /// Faster Airbraking
        /// </summary>
        private static void OnAirBrake(On.Vulture.orig_AirBrake orig, Vulture self, int frames)
        {
            // Faster Air Braking
            if(self.Template.type == DLCreature.Buzzard)
            {
                self.landingBrake = frames - 5;
                self.landingBrakePos = self.bodyChunks[1].pos;
                if (frames > 5)
                {
                    self.room.PlaySound(SoundID.Vulture_Jets_Air_Brake, self.mainBodyChunk, false, 1, 1.5f);
                }
                return;
            }
            orig(self, frames);
        }

        private static void OnVultureCtor(On.Vulture.orig_ctor orig, Vulture self, AbstractCreature abstractCreature, World world)
        {
            orig(self, abstractCreature, world);
            if (self.Template.type == DLCreature.Buzzard)
            {
                foreach (var chunk in self.bodyChunks)
                {
                    chunk.rad = chunk.rad - 2.5f;
                }
                BuzzardModule.Add(self, new BuzzardModule(self));
            }
        }

        private static void OnVultureAICtor(On.VultureAI.orig_ctor orig, VultureAI self, AbstractCreature creature, World world)
        {
            orig(self, creature, world);
            if (creature.creatureTemplate.type == DLCreature.Buzzard)
            {
                self.pathFinder.accessibilityStepsPerFrame = 70;
                self.pathFinder.stepsPerFrame = 60;
                foreach (var module in self.modules)
                {
                    if (module is PreyTracker preyTracker)
                    {
                        preyTracker.persistanceBias = 2f;
                    }
                }
                //self.pathFinder.visualize = true;
            }
        }
        /// <summary>
        /// Makes Buzzard's less disencouraged when attacked
        /// </summary>
        private static void ILVultureViolence(ILContext il)
        {
            var c = new ILCursor(il);

            if (c.TryGotoNext(MoveType.After, // Less disencouragement which means more presistance
                x => x.MatchCallvirt(typeof(VultureAI).GetMethod("set_disencouraged", new[] { typeof(float) }))))
            {
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate<Action<Vulture>>((vulture) =>
                {
                    if (vulture.Template.type == DLCreature.Buzzard)
                    {
                        Debug.Log("Buzzard Violence!");
                        Debug.Log(vulture.AI);
                        Debug.Log(vulture.AI.GetType().Name);
                        vulture.AI.disencouraged = vulture.AI.disencouraged * 0.15f;
                    }
                });
            }
            else
            {
                Plugin.Logger.LogFatal("ILVultureViolence failed!");
            }
        }
        /// <summary>
        /// Additional setup for Buzzard's
        /// </summary>
        private static void OnVultureGraphicsCtor(On.VultureGraphics.orig_ctor orig, VultureGraphics self, Vulture ow)
        {
            orig(self, ow);
            if (self.vulture.Template.type == DLCreature.Buzzard)
            {
                Random.State state = Random.state;
                Random.InitState(self.vulture.abstractCreature.ID.RandomSeed);
                BuzzardModule.TryGetValue(self.vulture, out var data);
                if (data != null)
                {
                    data.InitiateGraphics(self);
                }
                self.DEBUGLABELS = new DebugLabel[1];
                self.DEBUGLABELS[0] = new DebugLabel(ow, new Vector2(40f, 50f));
                self.ColorA = new HSLColor(Mathf.Lerp(0.9f, 1.6f, Random.value), Mathf.Lerp(0.2f, 0.3f, Random.value), Mathf.Lerp(0.7f, 0.8f, Random.value));
                self.ColorB = new HSLColor(self.ColorA.hue + Mathf.Lerp(-0.25f, 0.25f, Random.value), Mathf.Lerp(0.4f, 0.8f, 1f - Random.value * Random.value), Mathf.Lerp(0.45f, 1f, Random.value * Random.value));
                self.eyeCol = Custom.HSL2RGB(Mathf.Lerp(0.08f, 0.17f, Random.value), 1f, 0.6f);
                Random.state = state;
            }
        }

        private static void OnAddToContainer(On.VultureGraphics.orig_AddToContainer orig, VultureGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
        {
            orig(self, sLeaser, rCam, newContatiner);
            /*
            if (self.vulture.Template.type == Type.Buzzard)
            {
                BuzzardModule.TryGetValue((self.owner as Vulture), out var data);
                if (data != null)
                {
                    for(int i = 0; i < data.tail.Length; i++)
                    {
                        //data.tail[i].
                    }
                }
            }
            */
        }

        private static void OnInitiateSprites(On.VultureGraphics.orig_InitiateSprites orig, VultureGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            try
            {
                orig(self, sLeaser, rCam);
            } catch (Exception e)
            {
                Debug.Log(e);
                for (int i = 0; i < sLeaser.sprites.Length; i++)
                {
                    if (sLeaser.sprites[i] != null && sLeaser.sprites[i].element != null)
                    {
                        Debug.Log(i + " | " + sLeaser.sprites[i].element.name);
                    } else
                    {
                        Debug.Log(i + " | NULL!");
                    }
                }
            }
            Debug.Log(sLeaser.sprites.Length);
            if (self.vulture.Template.type == DLCreature.Buzzard)
            {
                BuzzardModule.TryGetValue((self.owner as Vulture), out var data);
                sLeaser.sprites[self.BodySprite].scale = 0.8f;
                
                for (int j = 0; j < self.vulture.tentacles.Length; j++)
                {
                    if (sLeaser.sprites[self.TentacleSprite(j)] is TriangleMesh)
                    {
                        
                    }
                    sLeaser.sprites[TotalBuzzardSprites(self) - 1] = TriangleMesh.MakeLongMesh(data.tail.Length, true, false);
                    
                }
                
                self.AddToContainer(sLeaser, rCam, null);
            }
            for(int i = 0; i < sLeaser.sprites.Length; i++)
            {
                Debug.Log(i + " | " + sLeaser.sprites[i].element.name);
            }
        }

        private static void ILInitiateSprites(ILContext il)
        {
            var c = new ILCursor(il);

            if (c.TryGotoNext(MoveType.After,
                x => x.MatchNewarr(typeof(FSprite)),
                x => x.MatchStfld(typeof(RoomCamera.SpriteLeaser).GetField("sprites"))))
            {
                c.Emit(OpCodes.Ldarg_1);
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate<Action<RoomCamera.SpriteLeaser, VultureGraphics>>((sLeaser, graphics) => {
                    if (graphics.owner is Vulture && (graphics.owner as Vulture).Template.type == DLCreature.Buzzard)
                    {
                        sLeaser.sprites = new FSprite[TotalBuzzardSprites(graphics)];
                    }
                });
            }

        }
        /// <summary>
        /// Removes all vulture feathers on Buzzards since they use a different wing type.
        /// </summary>
        private static void ILVultureGraphicsCtor(ILContext il)
        {
            var c = new ILCursor(il);
            if (c.TryGotoNext(MoveType.After,
                x => x.MatchLdcI4(0x19),
                x => x.MatchCall(typeof(UnityEngine.Random).GetMethod("Range", new[] { typeof(int), typeof(int) })),
                x => x.MatchStfld(typeof(VultureGraphics).GetField("feathersPerWing"))))
            {
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate<Action<VultureGraphics>>((graphics) =>
                {
                    if (graphics.vulture.Template.type == DLCreature.Buzzard)
                    {
                        graphics.feathersPerWing = 0;
                    }
                });
            }
            else
            {
                Plugin.Logger.LogFatal("ILVultureGraphicsCtor failed!");
            }
        }
        /*
        private static void ILInitiateSprites(ILContext il)
        {
            var c = new ILCursor(il);
            if (c.TryGotoNext(MoveType.After,
                x => x.MatchLdcR4(1.2f),
                x => x.MatchCallvirt(typeof(FNode).GetMethod("set_scale"))))
            {
                c.Emit(OpCodes.Ldarg_0);
                c.Emit(OpCodes.Ldfld, typeof(RoomCamera.SpriteLeaser).GetField("sprites"));
                c.EmitDelegate<Action<VultureGraphics, FSprite[]>>((graphics, sprites) =>
                {
                    Debug.Log(sprites);
                    if (graphics.vulture.Template.type == Type.Buzzard)
                    {
                        sprites[graphics.BodySprite].scale = 0.8f;
                    }
                });
            }
            else
            {
                Plugin.Logger.LogFatal("ILInitiateSprites failed!");
            }
        }
        */
        #endregion

        /// <summary>
        /// An attempt to make corpses more attractive. Not entirely sure if it works or not atm.
        /// </summary>
        private static void ILAttractiveness(ILContext il)
        {
            var c = new ILCursor(il);

            if (c.TryGotoNext(MoveType.Before,
                x => x.MatchLdloc(0),
                x => x.MatchRet()))
            {
                c.Emit(OpCodes.Ldarg_0);
                c.Emit(OpCodes.Ldloc_0);
                c.EmitDelegate<Func<PreyTracker.TrackedPrey, float, float>>((trackedPrey, num) =>
                {
                    if (trackedPrey.owner.AI.creature.creatureTemplate.type == DLCreature.Buzzard && trackedPrey.critRep.representedCreature.realizedCreature != null
                    && trackedPrey.critRep.representedCreature.realizedCreature.dead)
                    {
                        return num * 2; // A creature's attractiveness for the Buzzard will be doubled if it's dead.
                    }
                    return num;
                });
                c.Emit(OpCodes.Stloc_0);
            }
            else
            {
                Plugin.Logger.LogFatal("ILAttractiveness failed!");
            }
        }

        private static void OnReleaseGrip(On.VultureTentacle.orig_ReleaseGrip orig, VultureTentacle self)
        {
            if (self.vulture.Template.type == DLCreature.Buzzard)
            {
                if (self.OtherTentacle.grabDelay < 1)
                {
                    self.grabDelay = 20;
                }
                self.floatGrabDest = null;
            } else
            {
                orig(self);
            }
        }

    }
}
