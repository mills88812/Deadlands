using Deadlands;
using Deadlands.Creatures.Buzzard;
using IL;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RWCustom;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;
using static Watcher.FireSpriteGraphics;
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
            On.Vulture.Carry += OnVultureCarry;
            On.Vulture.AirBrake += OnAirBrake;
            On.Vulture.UpdateNeck += OnUpdateNeck;
            On.Weapon.Shoot += OnShootBuzzard;

            IL.Vulture.ctor += ILVutureCtor;
            IL.Vulture.Violence += ILVultureViolence;

            // State

            On.Vulture.VultureState.ctor += OnVultureStateCtor;

            // AI

            On.VultureAI.ctor += OnVultureAICtor;

            // Graphics

            On.VultureGraphics.ctor += OnVultureGraphicsCtor;
            On.VultureGraphics.InitiateSprites += OnInitiateSprites;
            On.VultureGraphics.DrawSprites += OnDrawSprites;
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
        private static void OnVultureCarry(On.Vulture.orig_Carry orig, Vulture self)
        {
            if (self.Template.type != DLCreature.Buzzard)
            {
                orig(self);
            }
            else
            {
                if (!self.Consious)
                {
                    self.LoseAllGrasps();
                    return;
                }
                BodyChunk grabbedChunk = self.grasps[0].grabbedChunk;
                float num = 1f;

                if (Random.value < 0.008333334f * num && (!(grabbedChunk.owner is Creature) || self.Template.CreatureRelationship((grabbedChunk.owner as Creature).Template).type != CreatureTemplate.Relationship.Type.Eats))
                {
                    if(!(grabbedChunk.owner is Weapon))
                    {
                        self.LoseAllGrasps();
                        return;
                    }
                }

                float num2 = grabbedChunk.mass / (grabbedChunk.mass + self.bodyChunks[4].mass);
                float num3 = grabbedChunk.mass / (grabbedChunk.mass + self.bodyChunks[0].mass);
                if (self.neck.backtrackFrom != -1 || self.enteringShortCut != null)
                {
                    num2 = 0f;
                    num3 = 0f;
                }
                
                if (!Custom.DistLess(grabbedChunk.pos, self.neck.tChunks[self.neck.tChunks.Length - 1].pos, 20f))
                {
                    Vector2 a = Custom.DirVec(grabbedChunk.pos, self.neck.tChunks[self.neck.tChunks.Length - 1].pos);
                    float num4 = Vector2.Distance(grabbedChunk.pos, self.neck.tChunks[self.neck.tChunks.Length - 1].pos);
                    grabbedChunk.pos -= (20f - num4) * a * (1f - num2);
                    grabbedChunk.vel -= (20f - num4) * a * (1f - num2);
                    self.neck.tChunks[self.neck.tChunks.Length - 1].pos += (20f - num4) * a * num2;
                    self.neck.tChunks[self.neck.tChunks.Length - 1].vel += (20f - num4) * a * num2;
                }
                if (self.enteringShortCut == null)
                {
                    self.bodyChunks[4].pos = Vector2.Lerp(self.neck.tChunks[self.neck.tChunks.Length - 1].pos, grabbedChunk.pos, 0.1f);
                    self.bodyChunks[4].vel = self.neck.tChunks[self.neck.tChunks.Length - 1].vel;
                }
                float num5 = 70f;
                if (!Custom.DistLess(self.mainBodyChunk.pos, grabbedChunk.pos, num5))
                {
                    Vector2 a2 = Custom.DirVec(grabbedChunk.pos, self.bodyChunks[0].pos);
                    float num6 = Vector2.Distance(grabbedChunk.pos, self.bodyChunks[0].pos);
                    grabbedChunk.pos -= (num5 - num6) * a2 * (1f - num3);
                    grabbedChunk.vel -= (num5 - num6) * a2 * (1f - num3);
                    self.bodyChunks[0].pos += (num5 - num6) * a2 * num3;
                    self.bodyChunks[0].vel += (num5 - num6) * a2 * num3;
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
        public static float WingRadiusAlong(float f)
        {
            return Mathf.Lerp(0.25f, 15f, Mathf.Pow(Mathf.Clamp01(Mathf.Sin(Mathf.Pow((1f - f) * 0.95f, 2f) * 3.1415927f)), 0.5f));
        }
        private static void OnDrawSprites(On.VultureGraphics.orig_DrawSprites orig, VultureGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            orig(self, sLeaser, rCam, timeStacker, camPos);
            // Post DrawSprites, Not currently in use
            
            if (self.vulture.Template.type == DLCreature.Buzzard)
            {

                FSprite fsprite9 = sLeaser.sprites[self.MaskSprite];
                fsprite9.element = Futile.atlasManager.GetElementWithName(string.Format("Buzard_Mask{0}", self.headGraphic));
                for (int k = 0; k < self.vulture.tentacles.Length; k++)
                {
                    Vector2 vector6 = Vector2.Lerp(self.vulture.tentacles[k].connectedChunk.lastPos, self.vulture.tentacles[k].connectedChunk.pos, timeStacker);
                    

                }
            }
            
        }
        public static Color SetColorAlpha(Color c, float a)
        {
            return new Color(c.r, c.g, c.b, a);
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
        private static void OnUpdateNeck(On.Vulture.orig_UpdateNeck orig, Vulture self)
        {
            // Faster Air Braking
            if (self.Template.type != DLCreature.Buzzard)
            {
                orig(self);
            }
            else
            {
                self.neck.Update();
                if (self.AI.stuckTracker.closeToGoalButNotSeeingItTracker.counter > self.AI.stuckTracker.closeToGoalButNotSeeingItTracker.counterMin)
                {
                    List<IntVector2> list = null;
                    float num = self.AI.stuckTracker.closeToGoalButNotSeeingItTracker.Stuck;
                    self.neck.MoveGrabDest(self.room.MiddleOfTile(self.AI.pathFinder.GetDestination), ref list);
                    Vector2 b = Custom.DirVec(self.bodyChunks[4].pos, self.room.MiddleOfTile(self.AI.pathFinder.GetDestination)) * (10f * num);
                    self.bodyChunks[4].vel += b;
                    self.bodyChunks[4].pos += b;
                    for (int i = 0; i < self.neck.tChunks.Length; i++)
                    {
                        self.neck.tChunks[i].vel += Custom.DirVec(self.neck.tChunks[i].pos, self.room.MiddleOfTile(self.AI.pathFinder.GetDestination)) * (5f * num);
                    }
                    if (num > 0.95f)
                    {
                        self.bodyChunks[4].collideWithTerrain = false;
                        return;
                    }
                }
                self.bodyChunks[4].collideWithTerrain = true;
                for (int j = 0; j < self.neck.tChunks.Length; j++)
                {
                    self.neck.tChunks[j].vel *= 0.95f;
                    Tentacle.TentacleChunk tentacleChunk = self.neck.tChunks[j];
                    tentacleChunk.vel.y = tentacleChunk.vel.y - (self.neck.limp ? 0.7f : 0.1f);
                    self.neck.tChunks[j].vel += Custom.DirVec(self.bodyChunks[1].pos, self.bodyChunks[0].pos) * ((j == 0) ? 1.2f : 0.8f);
                    self.neck.tChunks[j].vel -= self.neck.connectedChunk.vel;
                    self.neck.tChunks[j].vel *= (self.AirBorne ? 0.2f : 0.75f);
                    self.neck.tChunks[j].vel += self.neck.connectedChunk.vel;
                }
                self.neck.limp = !self.Consious;
                float num2 = (self.neck.backtrackFrom == -1) ? 0.5f : 0f;
                if (self.grasps[0] == null)
                {
                    Vector2 a = Custom.DirVec(self.bodyChunks[4].pos, self.neck.tChunks[self.neck.tChunks.Length - 1].pos);
                    float num3 = Vector2.Distance(self.bodyChunks[4].pos, self.neck.tChunks[self.neck.tChunks.Length - 1].pos);
                    Vector2 b2 = a * ((6f - num3) * (1f - num2));
                    Vector2 b3 = a * ((6f - num3) * num2);
                    self.bodyChunks[4].pos -= b2;
                    self.bodyChunks[4].vel -= b2;
                    self.neck.tChunks[self.neck.tChunks.Length - 1].pos += b3;
                    self.neck.tChunks[self.neck.tChunks.Length - 1].vel += b3;
                    self.bodyChunks[4].vel += Custom.DirVec(self.neck.tChunks[self.neck.tChunks.Length - 2].pos, self.bodyChunks[4].pos) * ((self.AirBorne ? 2f : 6f) * (1f - num2));
                    self.bodyChunks[4].vel += Custom.DirVec(self.neck.tChunks[self.neck.tChunks.Length - 1].pos, self.bodyChunks[4].pos) * ((self.AirBorne ? 2f : 6f) * (1f - num2));
                    Vector2 b4 = Custom.DirVec(self.neck.tChunks[self.neck.tChunks.Length - 2].pos, self.bodyChunks[4].pos) * ((self.AirBorne ? 1f : 3f) * num2);
                    self.neck.tChunks[self.neck.tChunks.Length - 1].vel -= b4;
                    self.neck.tChunks[self.neck.tChunks.Length - 2].vel -= b4;
                }
                if (!self.Consious)
                {
                    return;
                }
                Vector2 pos = self.snapAtPos;
                if (self.snapAt != null)
                {
                    pos = self.snapAt.pos;
                }
                if (self.ChargingSnap)
                {
                    self.bodyChunks[4].vel += (self.mainBodyChunk.pos + Custom.DirVec(self.mainBodyChunk.pos, pos) * 50f - self.bodyChunks[4].pos) / 6f;
                    self.neck.tChunks[self.neck.tChunks.Length - 1].vel -= Custom.DirVec(self.bodyChunks[4].pos, pos) * (10f * num2);
                    return;
                }
                if (self.Snapping)
                {
                    self.bodyChunks[4].vel += Custom.DirVec(self.bodyChunks[4].pos, pos) * 15f;
                    self.neck.tChunks[self.neck.tChunks.Length - 1].vel -= Custom.DirVec(self.bodyChunks[4].pos, pos) * num2;
                    return;
                }
                Vector2 vector;
                
                if (self.AI.creatureLooker.lookCreature == null)
                {
                    vector = self.room.MiddleOfTile(self.AI.pathFinder.GetDestination);
                }
                else if((self.AI as BuzzardAI).focusWepon != null && self.AI.creatureLooker.lookCreature.representedCreature.realizedCreature != null && self.grasps[0] == null && Custom.DistLess(self.mainBodyChunk.pos, (self.AI as BuzzardAI).focusWepon.representedItem.realizedObject.bodyChunks[0].pos, Custom.Dist(self.mainBodyChunk.pos, self.AI.creatureLooker.lookCreature.representedCreature.realizedCreature.bodyChunks[0].pos)))
                {
                    vector = self.room.MiddleOfTile((self.AI as BuzzardAI).focusWepon.BestGuessForPosition());
                }
                else if (self.AI.creatureLooker.lookCreature.VisualContact)
                {
                    vector = self.AI.creatureLooker.lookCreature.representedCreature.realizedCreature.DangerPos;
                }
                else
                {
                    vector = self.room.MiddleOfTile(self.AI.creatureLooker.lookCreature.BestGuessForPosition());
                }
                if (Custom.DistLess(vector, self.mainBodyChunk.pos, 220f) && !self.room.VisualContact(vector, self.bodyChunks[4].pos))
                {
                    List<IntVector2> list2 = null;
                    self.neck.MoveGrabDest(vector, ref list2);
                }
                else if (self.neck.backtrackFrom == -1)
                {
                    self.neck.floatGrabDest = null;
                }
                Vector2 a2 = Custom.DirVec(self.bodyChunks[4].pos, vector);
                if (self.grasps[0] == null)
                {
                    self.neck.tChunks[self.neck.tChunks.Length - 1].vel += a2 * num2;
                    self.neck.tChunks[self.neck.tChunks.Length - 2].vel -= a2 * (0.5f * num2);
                    self.bodyChunks[4].vel += a2 * (4f * (1f - num2));
                }
                else
                {
                    Vector2 b5 = a2 * (2f * num2);
                    self.neck.tChunks[self.neck.tChunks.Length - 1].vel += b5;
                    self.neck.tChunks[self.neck.tChunks.Length - 2].vel -= b5; 
                    if (self.grasps[0].grabbedChunk.owner is not Weapon)
                    {
                        self.grasps[0].grabbedChunk.vel += a2 / self.grasps[0].grabbedChunk.mass;
                    }
                }
                if (Custom.DistLess(self.bodyChunks[4].pos, vector, 80f))
                {
                    for (int k = 0; k < self.neck.tChunks.Length; k++)
                    {
                        self.neck.tChunks[k].vel -= a2 * (Mathf.InverseLerp(80f, 20f, Vector2.Distance(self.bodyChunks[4].pos, vector)) * 8f * num2);
                    }
                }
            }

        }

        public static void OnShootBuzzard(On.Weapon.orig_Shoot orig, Weapon self, Creature shotBy, Vector2 thrownPos, Vector2 throwDir, float force, bool eu)
        {
            orig(self, shotBy, thrownPos, throwDir, force, eu);

            if(shotBy.Template.type == DLCreature.Buzzard)
            {
                if(self is Spear)
                {
                    Room room2 = self.room;
                    if (room2 != null)
                    {
                        room2.PlaySound(SoundID.Slugcat_Throw_Spear, self.firstChunk);
                    }

                    (self as Spear).alwaysStickInWalls = false;
                   
                }
                if (self is Rock)
                {
                    Room room = self.room;
                    if (room == null)
                    {
                        return;
                    }
                    room.PlaySound(SoundID.Slugcat_Throw_Rock, self.firstChunk);

                }
                if (self is ScavengerBomb)
                {
                    Room room = self.room;
                    if (room != null)
                    {
                        room.PlaySound(SoundID.Slugcat_Throw_Bomb, self.firstChunk);
                    }
                    (self as ScavengerBomb).ignited = true;

                }
                if (self is SporePlant)
                {
                    if (!(self as SporePlant).Used)
                    {
                        (self as SporePlant).deployOnCollision = true;
                    }
                     (self as SporePlant).Pacified = false;

                }
            }
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
        private static CreatureTemplate.Relationship UpdateDynamicRelationship(On.VultureAI.orig_IUseARelationshipTracker_UpdateDynamicRelationship orig, VultureAI self, RelationshipTracker.DynamicRelationship dRelation)
        {
            CreatureTemplate.Relationship currentRelationship = orig.Invoke(self, dRelation);
            if (dRelation.trackerRep is Tracker.SimpleCreatureRepresentation)
            {
                return currentRelationship;
            }
            if (self.preyTracker.MostAttractivePrey != null && self.preyTracker.MostAttractivePrey.representedCreature == dRelation.trackerRep.representedCreature && currentRelationship.type == CreatureTemplate.Relationship.Type.Uncomfortable)
            {
                currentRelationship.type = CreatureTemplate.Relationship.Type.Eats;
                currentRelationship.intensity = 1f;
            }
            if (currentRelationship.type == CreatureTemplate.Relationship.Type.Uncomfortable || dRelation.trackerRep.representedCreature.creatureTemplate.type == DLCSharedEnums.CreatureTemplateType.Inspector)
            {
                if (dRelation.trackerRep.VisualContact)
                {
                    if (dRelation.trackerRep.representedCreature.realizedCreature != null && !dRelation.trackerRep.representedCreature.realizedCreature.dead)
                    {
                        Creature realizedCreature = dRelation.trackerRep.representedCreature.realizedCreature;
                        if (realizedCreature.grasps != null && realizedCreature.grasps.Length != 0)
                        {
                            for (int i = 0; i < realizedCreature.grasps.Length; i++)
                            {
                                if (realizedCreature.grasps[i] != null && realizedCreature.grasps[i].grabbed is SSOracleSwarmer)
                                {
                                    if ((realizedCreature.grasps[i].grabbed as SSOracleSwarmer).bites < 3)
                                    {
                                        currentRelationship.type = CreatureTemplate.Relationship.Type.Eats;
                                        currentRelationship.intensity = 1f;
                                        self.preyTracker.AddPrey(dRelation.trackerRep);
                                    }
                                    else
                                    {
                                        currentRelationship.intensity = 1f;
                                        if (!self.vulture.safariControlled)
                                        {
                                            Debug.Log("it gets to the grab1");
                                            (self as BuzzardAI).GrabObject(realizedCreature.grasps[i].grabbed);
                                        }
                                    }
                                }
                            }
                        }
                        currentRelationship.intensity = 1f;
                    }
                    if (currentRelationship.intensity < 0.5f && Random.value < 0.02f)
                    {
                        currentRelationship.intensity = 1f;
                    }
                    if (Vector2.Distance(self.vulture.mainBodyChunk.pos, dRelation.trackerRep.lastSeenCoord.Tile.ToVector2() * 20f) < 100f && currentRelationship.intensity + 0.09f < 1f)
                    {
                        currentRelationship.intensity += 0.09f;
                    }
                }
                if (currentRelationship.intensity > 0f && dRelation.trackerRep.VisualContact)
                {
                    currentRelationship.intensity -= 0.006f;
                }
                if (currentRelationship.intensity > 0f && !dRelation.trackerRep.VisualContact)
                {
                    currentRelationship.intensity -= 0.01f;
                }
                if (currentRelationship.intensity < 0f)
                {
                    currentRelationship.intensity = 0f;
                }
            }
            else if (currentRelationship.type == CreatureTemplate.Relationship.Type.Attacks && dRelation.trackerRep.VisualContact)
            {
                
                    
                        currentRelationship.intensity += 0.08f;
                        if (currentRelationship.intensity > 1f)
                        {
                            currentRelationship.intensity = 1f;
                            self.behavior = BuzzardAI.Behavior.EscapeRain;
                            //self.newIdlePosCounter = UnityEngine.Random.Range(300, 400);
                        }
                    
                
                
            }
            else if (currentRelationship.type == CreatureTemplate.Relationship.Type.Eats && dRelation.trackerRep.VisualContact)
            {
                currentRelationship.intensity += 0.05f;
                BuzzardModule.TryGetValue(self.vulture, out var module);
                if (module != null)
                {

                try
                {

                    
                    if (currentRelationship.intensity >= 1f)
                    {
                        Creature realizedCreature2 = dRelation.trackerRep.representedCreature.realizedCreature;
                        if (realizedCreature2.abstractCreature.creatureTemplate.TopAncestor().type != CreatureTemplate.Type.DaddyLongLegs)
                        {
                            if (currentRelationship.intensity >= 1f)
                            {
                                int num2 = -1;
                                
                                if (num2 == -1)
                                {
                                    if (!self.vulture.safariControlled)
                                    {
                                            Debug.Log("it gets to the grab3");
                                            (self as BuzzardAI).GrabObject(realizedCreature2);
                                    }
                                }
                                else
                                {
                                    if (self.vulture.neck.Tip.vel.magnitude < 1f || self.vulture.neck.Tip.vel.magnitude > 4f)
                                    {
                                        self.vulture.neck.Tip.vel *= 1.2f;
                                        self.vulture.neck.Tip.vel += new Vector2((float)Random.Range(-18, 18), (float)Random.Range(-18, 18));
                                        realizedCreature2.firstChunk.pos = self.vulture.neck.Tip.pos;
                                        realizedCreature2.firstChunk.vel = self.vulture.neck.Tip.vel;
                                    }
                                    float target = Custom.VecToDeg(Custom.DirVec(self.vulture.mainBodyChunk.pos, realizedCreature2.firstChunk.pos));
                                    bool flag = false;
                                    float num3 = 2000f;
                                    /*for (int k = 0; k < this.myInspector.DangerousThrowLocations.Count; k++)
                                    {
                                        Vector2 vector = Vector2.Lerp(realizedCreature2.firstChunk.pos, this.myInspector.DangerousThrowLocations[k], 0.8f);
                                        if (UnityEngine.Random.value < 0.85f && Vector2.Distance(self.vulture.mainBodyChunk.pos, this.myInspector.DangerousThrowLocations[k]) < num3 && self.vulture.room.RayTraceTilesForTerrain((int)(realizedCreature2.firstChunk.pos.x / 20f), (int)(realizedCreature2.firstChunk.pos.y / 20f), (int)(vector.x / 20f), (int)(vector.y / 20f)))
                                        {
                                            num3 = Vector2.Distance(self.vulture.mainBodyChunk.pos, this.myInspector.DangerousThrowLocations[k]);
                                            flag = true;
                                            target = Custom.VecToDeg(Custom.DirVec(realizedCreature2.firstChunk.pos, this.myInspector.DangerousThrowLocations[k]));
                                        }
                                    }*/
                                    float num4 = 35f;
                                    if (flag)
                                    {
                                        num4 = 10f;
                                    }
                                    if (!flag && !self.vulture.room.RayTraceTilesForTerrain((int)(realizedCreature2.firstChunk.pos.x / 20f), (int)(realizedCreature2.firstChunk.pos.y / 20f), (int)(realizedCreature2.firstChunk.pos.x + realizedCreature2.firstChunk.vel.x * 3f / 20f), (int)(realizedCreature2.firstChunk.pos.y + realizedCreature2.firstChunk.vel.y * 3f / 20f)) && (realizedCreature2.firstChunk.vel.magnitude > 50f || (Random.value < 0.5f && Mathf.DeltaAngle(Custom.VecToDeg(realizedCreature2.firstChunk.vel), target) < num4 && realizedCreature2.firstChunk.vel.magnitude > 30f)))
                                    {
                                        currentRelationship.intensity = 0f;
                                        module.wantToGrabChunk = null;
                                        module.grabChunk = null;
                                        self.vulture.room.PlaySound(SoundID.Vulture_Peck, self.vulture.neck.Tip.pos, self.vulture.abstractCreature);
                                    }
                                    else if (Mathf.DeltaAngle(Custom.VecToDeg(realizedCreature2.firstChunk.vel), target) < num4 && realizedCreature2.firstChunk.vel.magnitude > 20f)
                                    {
                                        currentRelationship.intensity = 0f;
                                        module.wantToGrabChunk = null;
                                        module.grabChunk = null;
                                        self.vulture.room.PlaySound(SoundID.Vulture_Peck, self.vulture.neck.Tip.pos, self.vulture.abstractCreature);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        currentRelationship.intensity += 0.03f;
                        if (currentRelationship.intensity > 1f)
                        {
                            currentRelationship.intensity = 1f;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError("Error running Act");
                    Debug.LogError(ex);
                }

            }
            }
            return currentRelationship;
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
                       /* Debug.Log("Buzzard Violence!");
                        Debug.Log(vulture.AI);
                        Debug.Log(vulture.AI.GetType().Name);*/
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

                    sLeaser.sprites[self.TentacleSprite(j)] = TriangleMesh.MakeLongMesh(self.vulture.tentacles[j].tChunks.Length, false, true);

                    //sLeaser.sprites[self.TentacleSprite(j)].shader = rCam.room.game.rainWorld.Shaders["MothWing"];
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
