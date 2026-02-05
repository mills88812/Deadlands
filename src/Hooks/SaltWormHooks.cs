using Deadlands;
using Deadlands.Creatures.SaltWorm;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Watcher;
using Random = UnityEngine.Random;

namespace Deadlands.Hooks
{
    internal class SaltWormHooks
    {

        

        public static void Apply()
        {
            // Centipede

            On.Centipede.ctor += OnCentipedeCtor;

            On.Centipede.GenerateSize += OnGenerateSize;

            On.Centipede.InitiateGraphicsModule += OnInitiateGraphicsModule;

            On.Centipede.NewRoom += OnCentipedeNewRoom;

            On.Centipede.Update += OnCentipedeUpdate;

            On.Centipede.Act += OnCentipedeAct;

            On.Centipede.Violence += OnCentipedeViolence;

            // CentipedeAI

            On.CentipedeAI.ctor += OnCentipedeAICtor;

            On.CentipedeAI.Update += OnCentipedeAIUpdate;

            On.CentipedeAI.TravelPreference += OnCentipedeAITravelPreference;

        }

        #region Centipede

        private static float OnGenerateSize(On.Centipede.orig_GenerateSize orig, AbstractCreature abstrCrit)
        {
            if (abstrCrit.creatureTemplate.type == DLCreature.SaltWorm)
            {
                Random.State state = Random.state;
                Random.InitState(abstrCrit.ID.RandomSeed);
                float result = result = Mathf.Lerp(0.9f, 1.7f, Random.value);
                Random.state = state;
                if (abstrCrit.spawnData != null && abstrCrit.spawnData.Length > 2)
                {
                    string s = abstrCrit.spawnData.Substring(1, abstrCrit.spawnData.Length - 2);
                    try
                    {
                        result = float.Parse(s, NumberStyles.Any, CultureInfo.InvariantCulture);
                    }
                    catch
                    {
                    }
                }
                return result;
            } else
            {
                return orig(abstrCrit);
            }
        }
        private static void OnInitiateGraphicsModule(On.Centipede.orig_InitiateGraphicsModule orig, Centipede self)
        {
            orig(self);
            if (self.Template.type == DLCreature.SaltWorm)
            {
                if (self.graphicsModule is not SaltWormGraphics)
                {
                    self.graphicsModule = new SaltWormGraphics(self);
                }
            }
        }
        private static void OnCentipedeCtor(On.Centipede.orig_ctor orig, Centipede self, AbstractCreature abstractCreature, World world)
        {
            orig(self, abstractCreature, world);
            if (self.Template.type == DLCreature.SaltWorm)
            {
                for (int i = 0; i < self.bodyChunks.Length; i++)
                {
                self.bodyChunks[i].mass += 0.04f + 0.08f * Mathf.Clamp01(Mathf.Sin(Mathf.InverseLerp(0f, (float)(self.bodyChunks.Length - 1), (float)i) * 3.1415927f));
                }
                self.burrowFriction = 0.7f;
                self.Template.visualRadius = 300f;
            }
        }
        private static void OnCentipedeNewRoom(On.Centipede.orig_NewRoom orig, Centipede self, Room newRoom)
        {
            orig(self, newRoom);
            if (self.Template.type == DLCreature.SaltWorm)
            {
                for (int i = 0; i < self.bodyChunks.Length; i++)
                {
                    if (ModManager.Watcher)
                    {
                        burrowSound = new StaticSoundLoop(WatcherEnums.WatcherSoundID.Skink_Dig_LOOP, self.bodyChunks[i].pos, self.room, 1f, 1f);
                        burrowDeepSound = new StaticSoundLoop(WatcherEnums.WatcherSoundID.Skink_Dig_Deep_LOOP, self.bodyChunks[i].pos, self.room, 1f, 1f);
                    }
                }
            }
        }
        private static void OnCentipedeUpdate(On.Centipede.orig_Update orig, Centipede self, bool eu)
        {
            orig(self, eu);
            if (self.Template.type == DLCreature.SaltWorm)
            {

                for (int k = 0; k < self.CentiState.shells.Length; k++)
                {
                    if (self.bodyChunks[k].buried && (!self.CentiState.shells[k] || self.CentiState.shells.Length != self.bodyChunks.Length))
                    {
                        self.CentiState.shells[k] = Random.value < 0.985f;
                    }
                }
                if (((self.burrowFriction == 0.7f) && self.HeadChunk.buried))
                {
                    if (ModManager.MMF)
                    {
                        self.buoyancy = 0.92f;
                    }
                }
                float num = 0f;
                if (self.room.terrain != null)
                {
                    num = Mathf.Clamp01((self.room.terrain.SnapToTerrain(self.HeadChunk.pos, false).y - self.HeadChunk.pos.y) / 200f);
                }
                float num2 = 0f;
                int num3 = 0;
                for (int i = 0; i < self.bodyChunks.Length; i++)
                {
                    if (self.bodyChunks[i].buried)
                    {
                        num2 += Vector2.Distance(self.bodyChunks[i].lastPos, self.bodyChunks[i].pos);
                        num3++;
                    }
                }
                if (num3 > 0)
                {
                    num2 /= (float)num3;
                }
                float num4 = Mathf.Min(num2 / 5f, 1f);
                if (num4 > burrowSoundSmooth)
                {
                    burrowSoundSmooth = Custom.LerpAndTick(burrowSoundSmooth, num4, 0.125f, 0.04f);
                }
                else
                {
                    burrowSoundSmooth = Custom.LerpAndTick(burrowSoundSmooth, num4, 0.025f, 0.01f);
                }
                if (self.HeadChunk.buried && UnityEngine.Random.value < burrowSoundSmooth * 0.055f)
                {
                    if (ModManager.Watcher)
                    {
                        self.room.PlaySound(WatcherEnums.WatcherSoundID.Skink_Scrape, self.HeadChunk.pos, (0.1f + 0.5f * burrowSoundSmooth) * (1f - num * 0.4f), 0.7f + burrowSoundSmooth * 0.5f - 0.4f * num);
                    }
                }
                burrowSound.volume = Mathf.Min(burrowSoundSmooth, 0.4f) / 0.4f * (1f - num * 0.53f) * 0.5f;
                burrowSound.pitch = 0.97f + burrowSoundSmooth * 0.07f;
                burrowDeepSound.volume = Mathf.Min(burrowSoundSmooth, 0.7f) / 0.7f * num;
                burrowDeepSound.pitch = 0.97f + burrowSoundSmooth * 0.04f;
                burrowSound.pos = self.bodyChunks[1].pos;
                burrowSound.Update();
                burrowDeepSound.pos = self.bodyChunks[0].pos;
                burrowDeepSound.Update();
            }
        }
        private static void OnCentipedeAct(On.Centipede.orig_Act orig, Centipede self)
        {
            orig(self);
            if (self.Template.type == DLCreature.SaltWorm)
            {
                MovementConnection movementConnection = (self.AI.pathFinder as StandardPather).FollowPath(self.room.GetWorldCoordinate(self.HeadChunk.pos), true);
                if (!burrowUpcoming && self.room.aimap.getAItile(movementConnection.destinationCoord).acc == AItile.Accessibility.Sand)
                {
                    burrowUpcoming = true;
                }
                bool flag6 = self.room.terrain != null && (self.Buried || burrowUpcoming);
                self.HeadChunk.burrow = flag6;
                for (int num7 = 1; num7 < self.bodyChunks.Length; num7++)
                {
                    if (self.bodyChunks[num7 - 1].buried == flag6)
                    {
                        self.bodyChunks[num7].burrow = flag6;
                    }
                }
                /*if (!self.moving && self.AI.preyTracker.MostAttractivePrey != null && self.AI.preyTracker.MostAttractivePrey.representedCreature.realizedCreature != null && self.AI.preyTracker.MostAttractivePrey.representedCreature.realizedCreature.room == self.room && self.AI.preyTracker.MostAttractivePrey.VisualContact && self.grasps[0] == null && self.grasps[1] == null)
                {
                    //if (charging > 0f)
                    //{
                    //this.sitting = true;self
                    //base.GoThroughFloors = false;
                    //charging += 0.05f;
                    for (int num7 = 0; num7 < self.bodyChunks.Length; num7++)
                    {
                        Vector2 a2 = Custom.DirVec(self.HeadChunk.pos, self.AI.preyTracker.MostAttractivePrey.representedCreature.realizedCreature.mainBodyChunk.pos);
                        self.HeadChunk.vel += a2 * Mathf.Pow(0.5f, 4f);
                        if (self.HeadChunk == self.bodyChunks[0]) 
                        {
                            self.bodyChunks[num7].vel -= a2 * Mathf.Lerp(0.7f, 2f, 0.5f);
                        }
                        else self.bodyChunks[self.bodyChunks.Length-num7].vel -= a2 * Mathf.Lerp(0.7f, 2f, 0.5f);


                    }
                    Attack(self);
                        
                    //}
                }*/
                /*self.gripPoint = null;
                self.narrowUpcoming = false;*/
            }

        }
        public static void Attack(Centipede self)
        {
            if (!self.safariControlled && (self.AI.preyTracker.MostAttractivePrey == null || !self.AI.preyTracker.MostAttractivePrey.VisualContact || /*!self.room.VisualContact(self.HeadChunk.pos, self.jumpAtPos) ||*/ self.AI.preyTracker.MostAttractivePrey.representedCreature.realizedCreature == null || self.AI.preyTracker.MostAttractivePrey.representedCreature.realizedCreature.room != self.room))
            {
                //self.charging = 0f;
                return;
            }
            Vector2 vector = Custom.DirVec(self.mainBodyChunk.pos, self.AI.preyTracker.MostAttractivePrey.representedCreature.realizedCreature.mainBodyChunk.pos);
        
                Vector2 vector2 = self.AI.preyTracker.MostAttractivePrey.representedCreature.realizedCreature.mainBodyChunk.pos;
                vector2 += new Vector2(0f, Mathf.InverseLerp(40f, 300f, Vector2.Distance(self.HeadChunk.pos, vector2)) * 40f);
                if (!Custom.DistLess(self.HeadChunk.pos, vector2, Custom.LerpMap(Vector2.Dot(vector, Custom.DirVec(self.HeadChunk.pos, vector2)), -1f, 1f, 0f, 500f)))
                {
                    //this.charging = 0f;
                    return;
                }
            
            /*this.jumpStamina = Mathf.Max(0f, this.jumpStamina - 0.35f);
            if (this.jumpStamina < 0.2f && UnityEngine.Random.value < 0.5f && !this.spitter)
            {
                this.AI.stayAway = true;
            }*/
            if (!self.room.GetTile(self.mainBodyChunk.pos + new Vector2(0f, 20f)).Solid && !self.room.GetTile(self.HeadChunk.pos + new Vector2(0f, 20f)).Solid)
            {
                vector = Vector3.Slerp(vector, new Vector2(0f, 1f), Custom.LerpMap(Vector2.Distance(self.HeadChunk.pos, self.AI.preyTracker.MostAttractivePrey.representedCreature.realizedCreature.mainBodyChunk.pos), 40f, 400f, 0.2f, 0.5f));
            }
            Jump(self,vector, 1f);
            self.LoseAllGrasps();
            //this.canBite = 40;
        }
        public static void Jump(Centipede self, Vector2 jumpDir, float soundVol)
        {
            float num = Custom.LerpMap(jumpDir.y, -1f, 1f, 0.7f, 1.2f, 1.1f);
            self.HeadChunk.vel *= 0.5f;
            self.HeadChunk.vel += jumpDir * (16f * num);
            for (int num7 = 0; num7 < self.bodyChunks.Length; num7++)
            {
                if (self.HeadChunk == self.bodyChunks[0])
                {
                    self.bodyChunks[num7].vel *= 0.5f;
                    self.bodyChunks[num7].vel += jumpDir * (11f * num);
                }
                else
                {
                    self.bodyChunks[self.bodyChunks.Length - num7].vel *= 0.5f;
                    self.bodyChunks[self.bodyChunks.Length - num7].vel += jumpDir * (11f * num);
                }
            }
            self.room.PlaySound(SoundID.Big_Spider_Jump, self.HeadChunk, false, soundVol, 1f);
        }
        private static void OnCentipedeViolence(On.Centipede.orig_Violence orig, Centipede self, BodyChunk source, Vector2? directionAndMomentum, BodyChunk hitChunk, PhysicalObject.Appendage.Pos hitAppendage, Creature.DamageType type, float damage, float stunBonus)
        {
            orig(self, source, directionAndMomentum, hitChunk, hitAppendage, type, damage, stunBonus);
            if (self.Template.type == DLCreature.SaltWorm)
            {
                if (hitChunk != null && hitChunk.index >= 0 && hitChunk.index < self.CentiState.shells.Length)
                {
                    if (self.CentiState.shells[hitChunk.index])
                    {
                        if (self.room != null)
                        {
                            self.shellJustFellOff = hitChunk.index;
                            self.CentiState.shells[hitChunk.index] = false;
                            if (self.graphicsModule != null)
                            {
                                for (int j = 0; j < (1); j++)
                                {
                                    self.room.AddObject(new SandPuffSpawner.SandPuff(hitChunk.pos, 8, 0f, true));
                                }
                            }
                        }
                    }
                }
            }
        }

        #endregion

        #region CentipedeAI

        private static void OnCentipedeAICtor(On.CentipedeAI.orig_ctor orig, CentipedeAI self, AbstractCreature creature, World world)
        {
            orig(self, creature, world);
            if (self.centipede.Template.type == DLCreature.SaltWorm)
            {
                self.pathFinder.stepsPerFrame = 20;
                self.preyTracker.sureToGetPreyDistance = 120;
            }
        }
        private static void OnCentipedeAIUpdate(On.CentipedeAI.orig_Update orig, CentipedeAI self)
        {
            orig(self);
            if (self.centipede.Template.type == DLCreature.SaltWorm)
            {

                if (self.noiseTracker != null)
                {
                    self.noiseTracker.hearingSkill = (self.centipede.moving ? 1f : 2.5f);
                }
            }
        }
        private static PathCost OnCentipedeAITravelPreference(On.CentipedeAI.orig_TravelPreference orig, CentipedeAI self, MovementConnection coord, PathCost cost)
        {

            if (self.centipede.Template.type == DLCreature.SaltWorm && self.pathFinder.GetDestination != default(WorldCoordinate) && self.centipede.room.terrain != null)
            {

                if (self.centipede.HeadChunk.burrow && self.centipede.room.aimap.getAItile(coord.destinationCoord).acc != AItile.Accessibility.Sand)
                {
                    cost.resistance += 5f;
                }
                int num2 = (int)(self.centipede.room.terrain.SnapToTerrain(coord.destinationCoord.Vec2(), false).y / 20f);
                if (coord.destinationCoord.y <= num2 && Custom.ManhattanDistance(coord.destinationCoord, self.pathFinder.GetDestination) > 5)
                {
                    cost.resistance += Mathf.Lerp(7f, 0f, Mathf.Min((float)(num2 - coord.destinationCoord.y), 4f) / 4f);
                }
            }
            return orig(self, coord, cost);
        }

        #endregion
    }
}
