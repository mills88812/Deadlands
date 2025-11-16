using RWCustom;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Messaging;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Deadlands.Creatures.CandleMouse
{
    internal class CandleMouse : AirBreatherCreature
    {
        public FlashTail flashTail { get { return (this.graphicsModule as CandleGraphics).tail; } }

        public CandleAI AI;
        public CandleMouse.IndividualVariations iVars;

        public float runSpeed;

        private int footingCounter;
        private int specialMoveCounter;
        public int voiceCounter;
        private IntVector2 specialMoveDestination;
        public float runCycle;
        private float profileFac;
        private MovementConnection lastFollowedConnection;
        private bool currentlyClimbingCorridor;
        public float burning;
        public float charge;

        public Vector2 lastFlickerDir;
        public Vector2 flickerDir;
        public float lastFlashAlpha;
        public float flashAplha;
        public float lastFlashRad;
        public float flashRad;
        public float flashAlpha;
        private bool carried;
        public float jumpCharging;
        private BodyChunk jumpAtChunk;
        public bool jumping;
        public bool sitting;
        private Vector2 jumpAtPos;

        public new CandleState State { get { return abstractCreature.state as CandleState; } }

        public bool Footing { get { return this.footingCounter > 10; } }

        public float LightIntensity { get { return Mathf.Pow(Mathf.Sin(this.burning * 3.1415927f), 0.4f); } }

        private void GenerateIVars()
        {
            Random.State state = Random.state;
            Random.InitState(abstractCreature.ID.RandomSeed);
            float hue;
            float sat = Mathf.Lerp(0.45f, 0.85f, Random.value);
            if (Random.value < 0.01f)
            {
                hue = Random.value;
            }
            else if (Random.value < 0.6f)
            {
                hue = Mathf.Lerp(0f, 0.1f, Random.value);
            }
            else
            {
                hue = Mathf.Lerp(0.5f, 0.85f, Random.value);
            }
            HSLColor color = new HSLColor(hue, sat, 0.8f);
            float value = Random.value;
            int tail = Random.Range(5, 7);
            this.iVars = new IndividualVariations(value, tail, color);
            Random.state = state;
        }

        public CandleMouse(AbstractCreature abstractCreature, World world) : base(abstractCreature, world)
        {
            this.GenerateIVars();
            float num = 0.4f;
            bodyChunks = new BodyChunk[2];
            bodyChunks[0] = new BodyChunk(this, 0, new Vector2(0, 0), 5f, num / 2f);
            bodyChunks[1] = new BodyChunk(this, 0, new Vector2(0, 0), 6f, num / 2f);
            this.mainBodyChunkIndex = 0;
            this.bodyChunkConnections = new PhysicalObject.BodyChunkConnection[1];
            this.bodyChunkConnections[0] = new PhysicalObject.BodyChunkConnection(bodyChunks[0], bodyChunks[1], 12f, PhysicalObject.BodyChunkConnection.Type.Normal, 1f, 0.5f);
            airFriction = 0.95f;
            gravity = 0.9f;
            this.bounce = 0.1f;
            this.surfaceFriction = 0.4f;
            this.collisionLayer = 1;
            waterFriction = 0.96f;
            buoyancy = 0.90f;
            this.jumpAtPos = Vector2.zero;
        }

        /// <summary>
        /// Blind immunity
        /// </summary>
        public override void Blind(int blnd)
        {
            blind = 0;
            //base.Blind(blnd);
        }
        public override void PlaceInRoom(Room placeRoom)
        {
            this.room = placeRoom;
            base.PlaceInRoom(placeRoom);
        }

        public override void Violence(BodyChunk source, Vector2? directionAndMomentum, BodyChunk hitChunk, Appendage.Pos hitAppendage, DamageType type, float damage, float stunBonus)
        {
            /*
            if (this.Consious && type == DamageType.Bite || type == DamageType.Electric || type == DamageType.Stab)
            {
                Ignite();
            }
            */
            base.Violence(source, directionAndMomentum, hitChunk, hitAppendage, type, damage, stunBonus);
            this.Squeak(1f);
        }

        public void Ignite(Creature? threat)
        {
            if (this.burning > 0f || this.charge < 2f)
            {
                return;
            }
            this.burning = 0.01f;
            //this.charge = 0f;
            this.room.PlaySound(SoundID.Flare_Bomb_Burn, this.flashTail.bulbPos);
            this.AI.behavior = CandleAI.Behavior.Flee;
        }

        public override void Update(bool eu)
        {
            if (!dead && this.State.health < 0f && Random.value < -this.State.health && Random.value < 0.025f)
            {
                this.Die();
                this.LoseAllGrasps();
            }
            if (!dead && Random.value * 0.7f > this.State.health && Random.value < 0.125f)
            {
                this.Stun(Random.Range(1, Random.Range(1, 27 - Custom.IntClamp((int)(20f * this.State.health), 0, 10))));
            }
            this.carried = this.grabbedBy.Count > 0;
            if (this.carried)
            {
                this.Carried();
            }
            //this.grabAttempts = Mathf.Max(0f, this.grabAttempts - 0.025f);
            base.Update(eu);
            if (this.room == null)
            {
                return;
            }
            this.sitting = false;
            if (this.burning > 0)
            {
                this.burning += 0.08f + (10f / (this.charge + 1f)) / 100f; // Burns out slightly faster if not fully charged. Adjust last variable to change rate.
                if (this.burning > 1f)
                {
                    this.burning = 0f;
                    this.charge = 0f;
                }
                this.lastFlickerDir = this.flickerDir;
                this.flickerDir = Custom.DegToVec(Random.value * 360f) * 50f * this.LightIntensity;
                this.lastFlashAlpha = this.flashAlpha;
                this.flashAplha = Mathf.Pow(Random.value, 0.3f) * this.LightIntensity;
                this.lastFlashRad = this.flashRad;
                this.flashRad = Mathf.Pow(Random.value, 0.3f) * this.LightIntensity * 200f * 16f;
                foreach(var creature in this.room.abstractRoom.creatures)
                {
                    if (creature.realizedCreature != null && (Custom.DistLess(base.firstChunk.pos, creature.realizedCreature.mainBodyChunk.pos, this.LightIntensity * 600f)
                        || (Custom.DistLess(base.firstChunk.pos, creature.realizedCreature.mainBodyChunk.pos, this.LightIntensity * 1600f)
                        && this.room.VisualContact(base.firstChunk.pos, creature.realizedCreature.mainBodyChunk.pos))))
                    {
                        if (creature.creatureTemplate.type == CreatureTemplate.Type.Spider && !creature.realizedCreature.dead)
                        {
                            creature.realizedCreature.firstChunk.vel += Custom.DegToVec(Random.value * 360f) * Random.value * 9f;
                            creature.realizedCreature.Die();
                        } else if (creature.creatureTemplate.type == CreatureTemplate.Type.BigSpider)
                        {
                            (creature.realizedCreature as BigSpider).poison = 1f;
                            (creature.realizedCreature as BigSpider).State.health -= Random.value * 0.4f;
                            creature.realizedCreature.Stun(Random.Range(15, 30));
                            // TODO Check if activated by player.
                            creature.realizedCreature.SetKillTag(this.abstractCreature);
                        }
                        creature.realizedCreature.Blind((int)Custom.LerpMap(Vector2.Distance(this.flashTail.bulbPos, creature.realizedCreature.VisionPoint), 60f, 600f, 400f, 20f));
                    }
                }
            }
            else
            {
                if (Consious && this.charge < 10f)
                {
                    this.charge += 0.01f;
                }
            }
            if (this.Consious)
            {
                this.footingCounter++;
                this.Act();
            } 
            else
            {
                this.footingCounter = 0;
                this.jumpCharging = 0;
                this.jumping = false;
            }
            if (this.Footing)
            {
                for (int i = 0; i < 2; i++)
                {
                    bodyChunks[i].vel *= 0.8f;
                    BodyChunk bodyChunk = bodyChunks[i];
                    bodyChunk.vel.y = bodyChunk.vel.y + gravity;
                }
            }
            if (Consious && !this.Footing && this.AI.behavior == CandleAI.Behavior.Flee && !safariControlled)
            {
                for (int j = 0; j < 2; j++)
                {
                    if (this.room.aimap.TileAccessibleToCreature(this.room.GetTilePosition(bodyChunks[j].pos), Template))
                    {
                        bodyChunks[j].vel += Custom.DegToVec(Random.value * 360f) * Random.value * 5f;
                    }
                }
            }
            if (this.grasps[0] != null)
            {
                this.CarryObject(eu);
            }
        }

        private void CarryObject(bool eu)
        {
            if (graphicsModule == null)
            {
                grasps[0].grabbedChunk.MoveFromOutsideMyUpdate(eu, bodyChunks[0].pos);
            }
            else
            {
                CandleGraphics graphics = graphicsModule as CandleGraphics;
                base.grasps[0].grabbedChunk.MoveFromOutsideMyUpdate(eu, base.bodyChunks[0].pos + graphics.bodyParts[1].vel.normalized * 10f);
            }
            grasps[0].grabbedChunk.vel = base.bodyChunks[1].vel;
            if (Vector2.Distance(base.grasps[0].grabbedChunk.pos, base.bodyChunks[0].pos) > 50f)
            {
                base.grasps[0].Release();
            }

        }

        private void Carried()
        {
            if (this.dead) return;
            var grabber = this.grabbedBy[0].grabber;
            if (grabber == null)
            {
                return;
            }
            if (!(grabber is Player) && this.AI.StaticRelationship(grabber.abstractCreature).type == CreatureTemplate.Relationship.Type.Afraid)
            {
                this.Ignite(grabber);
            }
            //if (this.room.aimap.TileAccessibleToCreature(this.bodyChunks[0].pos, this.Template) || this.room.aimap.TileAccessibleToCreature(this.bodyChunks[1].pos, this.Template))
        }

        private void Sit()
        {
            if (room.aimap.getAItile(bodyChunks[1].pos).acc == AItile.Accessibility.Floor && !IsTileSolid(0,0,1) && IsTileSolid(1,0,1))
            {
                BodyChunk mainBodyChunk = this.mainBodyChunk;
                mainBodyChunk.vel.y = mainBodyChunk.vel.y + 2f;
                BodyChunk bodyChunk = this.bodyChunks[1];
                bodyChunk.vel.y = bodyChunk.vel.y - 4f;
                this.profileFac *= 0.6f;
                this.sitting = true;
            }
        }

        private void Run(MovementConnection followingConnection)
        {
            float num = this.runCycle;
            this.runCycle += this.runSpeed / Mathf.Lerp(4f, 12f, Random.value);
            if (num < Mathf.Floor(this.runCycle))
            {
                this.room.PlaySound(SoundID.Mouse_Scurry, mainBodyChunk);
            }
            if (followingConnection.destinationCoord.x != followingConnection.startCoord.x)
            {
                if (followingConnection.destinationCoord.x > followingConnection.startCoord.x)
                {
                    this.profileFac = Mathf.Min(this.profileFac + 0.14285715f, 1f);
                }
                else
                {
                    this.profileFac = Mathf.Max(this.profileFac - 0.14285715f, -1f);
                }
            }
            else
            {
                this.profileFac = Mathf.Lerp(this.profileFac, -Custom.DirVec(bodyChunks[1].pos, mainBodyChunk.pos).x, 0.6f);
            }
            if (followingConnection.destinationCoord.y > followingConnection.startCoord.y && this.room.aimap.getAItile(followingConnection.destinationCoord).acc != AItile.Accessibility.Climb)
            {
                this.currentlyClimbingCorridor = true;
            }
            if (followingConnection.type == MovementConnection.MovementType.ReachUp)
            {
                (this.AI.pathFinder as StandardPather).pastConnections.Clear();
            }
            if (followingConnection.type == MovementConnection.MovementType.ShortCut || followingConnection.type == MovementConnection.MovementType.NPCTransportation)
            {
                this.enteringShortCut = new IntVector2?(followingConnection.StartTile);
                if (safariControlled)
                {
                    bool flag = false;
                    List<IntVector2> list = new List<IntVector2>();
                    foreach (ShortcutData shortcutData in this.room.shortcuts)
                    {
                        if (shortcutData.shortCutType == ShortcutData.Type.NPCTransportation && shortcutData.StartTile != followingConnection.StartTile)
                        {
                            list.Add(shortcutData.StartTile);
                        }
                        if (shortcutData.shortCutType == ShortcutData.Type.NPCTransportation && shortcutData.StartTile == followingConnection.StartTile)
                        {
                            flag = true;
                        }
                    }
                    if (flag)
                    {
                        if (list.Count > 0)
                        {
                            list.Shuffle<IntVector2>();
                            this.NPCTransportationDestination = this.room.GetWorldCoordinate(list[0]);
                        }
                        else
                        {
                            this.NPCTransportationDestination = followingConnection.destinationCoord;
                        }
                    }
                }
                else if (followingConnection.type == MovementConnection.MovementType.NPCTransportation)
                {
                    this.NPCTransportationDestination = followingConnection.destinationCoord;
                }
            }
            else if (followingConnection.type == MovementConnection.MovementType.OpenDiagonal || followingConnection.type == MovementConnection.MovementType.ReachOverGap || followingConnection.type == MovementConnection.MovementType.ReachUp || followingConnection.type == MovementConnection.MovementType.ReachDown || followingConnection.type == MovementConnection.MovementType.SemiDiagonalReach)
            {
                this.specialMoveCounter = 30;
                this.specialMoveDestination = followingConnection.DestTile;
            }
            else
            {
                Vector2 vector = this.room.MiddleOfTile(followingConnection.DestTile);
                if (this.lastFollowedConnection != null && this.lastFollowedConnection.type == MovementConnection.MovementType.ReachUp)
                {
                    bodyChunks[0].vel += Custom.DirVec(bodyChunks[0].pos, vector) * 4f;
                }
                if (this.Footing)
                {
                    for (int j = 0; j < 2; j++)
                    {
                        if (followingConnection.startCoord.x == followingConnection.destinationCoord.x)
                        {
                            BodyChunk bodyChunk = bodyChunks[j];
                            bodyChunk.vel.x = bodyChunk.vel.x + Mathf.Min((vector.x - bodyChunks[j].pos.x) / 8f, 1.2f);
                        }
                        else if (followingConnection.startCoord.y == followingConnection.destinationCoord.y)
                        {
                            BodyChunk bodyChunk2 = bodyChunks[j];
                            bodyChunk2.vel.y = bodyChunk2.vel.y + Mathf.Min((vector.y - bodyChunks[j].pos.y) / 8f, 1.2f);
                        }
                    }
                }
                if (this.lastFollowedConnection != null && (this.Footing || this.room.aimap.TileAccessibleToCreature(mainBodyChunk.pos, Template)) && ((followingConnection.startCoord.x != followingConnection.destinationCoord.x && this.lastFollowedConnection.startCoord.x == this.lastFollowedConnection.destinationCoord.x) || (followingConnection.startCoord.y != followingConnection.destinationCoord.y && this.lastFollowedConnection.startCoord.y == this.lastFollowedConnection.destinationCoord.y)))
                {
                    bodyChunks[0].vel *= 0.7f;
                    bodyChunks[1].vel *= 0.5f;
                }
                if (followingConnection.type == MovementConnection.MovementType.DropToFloor)
                {
                    this.footingCounter = 0;
                }
                this.MoveTowards(vector);
            }
            this.lastFollowedConnection = followingConnection;
        }

        private void Act()
        {
            this.AI.Update();
            if (this.jumping)
            {
                var foot = false;
                for (int i = 0; i < bodyChunks.Length; i++)
                {
                    if ((bodyChunks[i].ContactPoint.x != 0 || bodyChunks[i].ContactPoint.x != 0) && room.aimap.TileAccessibleToCreature(bodyChunks[i].pos, Template))
                    {
                        foot = true;
                    }
                }
                if (foot)
                {
                    footingCounter++;
                }
                else
                {
                    footingCounter = 0;
                }
                if (jumpAtChunk != null && room.VisualContact(mainBodyChunk.pos, jumpAtChunk.pos))
                {
                    bodyChunks[0].vel += Custom.DirVec(bodyChunks[0].pos, jumpAtChunk.pos) * 1.2f;
                    bodyChunks[1].vel -= Custom.DirVec(bodyChunks[0].pos, jumpAtChunk.pos) * 0.4f;
                }
                else if (jumpAtPos != Vector2.zero)
                {
                    bodyChunks[0].vel += Custom.DirVec(bodyChunks[0].pos, jumpAtPos) * 1.2f;
                    bodyChunks[1].vel -= Custom.DirVec(bodyChunks[0].pos, jumpAtPos) * 0.4f;
                }
                if (Footing)
                {
                    jumping = false;
                    jumpAtChunk = null;
                    jumpAtPos = Vector2.zero;
                }
            }
            /*
            for (int i = 0; i < AI.preyTracker.TotalTrackedPrey; i++)
            {
                var prey = AI.preyTracker.GetTrackedPrey(i);
                if (prey == null || prey.representedCreature.realizedCreature == null)
                {
                    continue;
                }
                var creature = prey.representedCreature.realizedCreature;
                foreach(var grasp in this.grasps)
                {
                    if (grasp != null && grasp.grabbed != null && grasp.grabbed == creature)
                    {
                        continue;
                    }
                }
                if (creature.room == this.room && this.room.GetTilePosition(this.mainBodyChunk.pos) == this.room.GetTilePosition(creature.mainBodyChunk.pos))
                {
                    if (this.AI.StaticRelationship(creature.abstractCreature).type == CreatureTemplate.Relationship.Type.Eats && Random.value < 0.75f)
                    {
                        if (this.Grab(creature, 0, 0, Creature.Grasp.Shareability.CanNotShare, 0.5f, false, true))
                        {
                            if (!creature.dead)
                            {
                                creature.Violence(base.mainBodyChunk, new Vector2?(Custom.DirVec(base.mainBodyChunk.pos, creature.bodyChunks[0].pos) * 0.1f), creature.bodyChunks[0], null, Creature.DamageType.Bite, 0.2f, 0f);
                            }
                            Debug.Log("CandleMouse grab");
                        }
                    }
                }
            }*/
            
            if (specialMoveCounter > 0)
            {
                specialMoveCounter--;
                this.MoveTowards(this.room.MiddleOfTile(this.specialMoveDestination));
                if (Custom.DistLess(bodyChunks[0].pos, this.room.MiddleOfTile(this.specialMoveDestination), 5f))
                {
                    this.specialMoveCounter = 0;
                }
            }
            else
            {
                if (!this.room.aimap.TileAccessibleToCreature(bodyChunks[0].pos, Template) && !this.room.aimap.TileAccessibleToCreature(bodyChunks[1].pos, Template))
                {
                    this.footingCounter = 0;
                }
                if ((!safariControlled && room.GetWorldCoordinate(mainBodyChunk.pos) == AI.pathFinder.GetDestination) || (!safariControlled && room.GetWorldCoordinate(bodyChunks[1].pos) == AI.pathFinder.GetDestination && AI.threatTracker.Utility() < 0.5f))
                {
                    this.Sit();
                    base.GoThroughFloors = false;
                }
                if (this.Footing && this.jumpCharging > 0f)
                {
                    this.sitting = true;
                    GoThroughFloors = false;
                    this.jumpCharging += 0.06666667f;
                    Vector2? vector = null;
                    if (this.jumpAtPos != Vector2.zero)
                    {
                        vector = new Vector2?(Custom.DirVec(base.mainBodyChunk.pos, this.jumpAtPos));
                    }
                    if (this.jumpAtChunk != null)
                    {
                        vector = new Vector2?(Custom.DirVec(base.mainBodyChunk.pos, this.jumpAtChunk.pos));
                    }
                    if (vector != null)
                    {
                        base.bodyChunks[0].vel += vector.Value * Mathf.Pow(this.jumpCharging, 2f);
                        base.bodyChunks[1].vel -= vector.Value * 4f * this.jumpCharging;
                    }
                    if (this.jumpCharging >= 1f)
                    {
                        this.Attack();
                    }
                }
                else
                {
                    MovementConnection movementConnection = (this.AI.pathFinder as StandardPather).FollowPath(this.room.GetWorldCoordinate(bodyChunks[0].pos), true);
                    if (movementConnection == null)
                    {
                        movementConnection = (this.AI.pathFinder as StandardPather).FollowPath(this.room.GetWorldCoordinate(bodyChunks[1].pos), true);
                    }
                    if (movementConnection != null)
                    {
                        this.Run(movementConnection);
                    }
                    else
                    {
                        GoThroughFloors = false;
                    }
                }
            }
            if (Consious)
            {
                this.profileFac *= 0.97f;
            }
            if (!Custom.DistLess(bodyChunks[0].pos, bodyChunks[0].lastPos, 5f))
            {
                this.runCycle += this.runSpeed / 40f;
            }
            if (this.voiceCounter > 0)
            {
                this.voiceCounter--;
                return;
            }
            if (!safariControlled && /*!this.Sleeping &&*/ Random.value < /*((this.ropeAttatchedPos != null) ? 0.1f : 1f)*/ 1 / ((this.AI.behavior == CandleAI.Behavior.Flee) ? Mathf.Lerp(80f, 20f, this.AI.threatTracker.Utility()) : 100f))
            {
                this.Squeak(Mathf.InverseLerp(0.5f, 1f, this.AI.threatTracker.Utility()));
            }
        }

        private void Attack()
        {
            if (this.grasps[0] != null || (this.jumpAtChunk == null && this.jumpAtPos == Vector2.zero) || this.jumpAtChunk.owner.room != this.room || !this.room.VisualContact(this.bodyChunks[0].pos, this.jumpAtChunk.pos))
            {
                this.jumpCharging = 0;
                this.jumpAtChunk = null;
                return;
            }
            Vector2? vector = null;
            if (this.jumpAtPos != Vector2.zero)
            {
                vector = new Vector2?(this.jumpAtPos);
            }
            if (this.jumpAtChunk != null)
            {
                vector = new Vector2?(this.jumpAtChunk.pos);
            }
            if (vector == null)
            {
                return;
            }
            Vector2 p = new Vector2(vector.Value.x, vector.Value.y);
            if (!this.room.GetTile(vector.Value + new Vector2(0f, 20f)).Solid)
            {
                Vector2? vector2 = vector;
                Vector2 b = new Vector2(0f, Mathf.InverseLerp(40f, 200f, Vector2.Distance(base.mainBodyChunk.pos, vector.Value)) * 20f);
                vector = vector2 + b;
            }
            Vector2 vector3 = Custom.DirVec(base.mainBodyChunk.pos, vector.Value);
            if (!Custom.DistLess(base.mainBodyChunk.pos, p, Custom.LerpMap(Vector2.Dot(vector3, Custom.DirVec(base.bodyChunks[1].pos, base.bodyChunks[0].pos)), -0.1f, 0.8f, 0f, 300f, 0.4f)))
            {
                this.jumpCharging = 0f;
                this.jumpAtChunk = null;
                return;
            }
            if (!this.room.GetTile(base.mainBodyChunk.pos + new Vector2(0f, 20f)).Solid && !this.room.GetTile(base.bodyChunks[1].pos + new Vector2(0f, 20f)).Solid)
            {
                vector3 = Vector3.Slerp(vector3, new Vector2(0f, 1f), Custom.LerpMap(Vector2.Distance(base.mainBodyChunk.pos, vector.Value), 40f, 200f, 0.05f, 0.2f));
            }
            this.room.PlaySound(SoundID.Slugcat_Super_Jump, this.bodyChunks[0]);
            this.room.PlaySound(SoundID.Mouse_Squeak, bodyChunks[0], false, 0.7f, 1f);
            this.Jump(vector3, jumpAtChunk);
        }
        protected void Squeak(float stress)
        {
            if (dead)
            {
                return;
            }
            if (this.voiceCounter > 0)
            {
                return;
            }
            this.room.PlaySound(SoundID.Mouse_Squeak, bodyChunks[0], false, Mathf.Lerp(0.5f, 1f, stress), Mathf.Lerp(1f, 1.3f, stress));
            this.voiceCounter = Random.Range(5, 12);
            if (graphicsModule != null)
            {
                (graphicsModule as CandleGraphics).head.pos += Custom.RNV() * 4f * Random.value;
                if (Random.value > Mathf.InverseLerp(0.5f, 1f, stress))
                {
                    //(graphicsModule as CandleGraphics).ouchEyes = Math.Max((graphicsModule as CandleGraphics).ouchEyes, this.voiceCounter);
                }
            }
        }

        private void MoveTowards(Vector2 moveTo)
        {
            Vector2 vector = Custom.DirVec(bodyChunks[0].pos, moveTo);
            if (this.room.aimap.getAItile(base.bodyChunks[1].pos).acc >= AItile.Accessibility.Climb)
            {
                vector *= 0.5f;
            }
            if (!this.Footing)
            {
                vector *= 0.3f;
            }
            if (IsTileSolid(1, 0, -1) && ((vector.x < -0.5 && bodyChunks[0].pos.x > bodyChunks[1].pos.x + 5f) || (vector.x > 0.5 && bodyChunks[0].pos.x < bodyChunks[1].pos.x - 5f)))
            {
                BodyChunk mainBodyChunk = this.bodyChunks[0];
                mainBodyChunk.vel.x = mainBodyChunk.vel.x - ((vector.x < 0f) ? -1f : 1f) * 1.3f;
                BodyChunk bodyChunk = bodyChunks[1];
                bodyChunk.vel.x = bodyChunk.vel.x + ((vector.x < 0f) ? -1f : 1f) * 0.5f;
                if (!IsTileSolid(0, 0, 1))
                {
                    BodyChunk mainBodyChunk2 = mainBodyChunk;
                    mainBodyChunk2.vel.y = mainBodyChunk2.vel.y + 3.2f;
                }
            }
            mainBodyChunk.vel += vector * 4.2f * this.runSpeed;
            bodyChunks[1].vel -= vector * 1f * this.runSpeed;
            GoThroughFloors = (moveTo.y < bodyChunks[0].pos.y - 5f);
        }

        public override void Collide(PhysicalObject otherObject, int myChunk, int otherChunk)
        {
            base.Collide(otherObject, myChunk, otherChunk);
            if (otherObject is IPlayerEdible && !(otherObject is Creature))
            {
                if (this.Grab(otherObject, 0, otherChunk, Creature.Grasp.Shareability.CanNotShare, 0.5f, false, true))
                {
                    Debug.Log("CandleMouse vegan grab");
                }
            }
            if (otherObject is Creature && this.AI.DynamicRelationship((otherObject as Creature).abstractCreature).type == CreatureTemplate.Relationship.Type.Afraid)
            {
                this.Ignite(otherObject as Creature);
            }
            if (otherObject is Creature && this.AI.StaticRelationship((otherObject as Creature).abstractCreature).type == CreatureTemplate.Relationship.Type.Eats)
            {
                if (this.jumping)
                {
                    this.mainBodyChunk.vel *= 0.5f;
                }
                if (this.Grab(otherObject, 0, otherChunk, Creature.Grasp.Shareability.CanNotShare, 1f, false, true))
                {
                    if (!(otherObject as Creature).dead)
                    {
                        (otherObject as Creature).Violence(base.mainBodyChunk, new Vector2?(Custom.DirVec(base.mainBodyChunk.pos, otherObject.bodyChunks[otherChunk].pos) * 0.1f),
                        otherObject.bodyChunks[otherChunk], null, Creature.DamageType.Bite, 0.2f, 0f);
                    }
                    Debug.Log("CandleMouse grab by collide");
                }
                //this.grabAttempts = 0;
                this.jumpCharging = 0;
                this.jumping = false;
            }
        }

        public override Color ShortCutColor()
        {
            return this.iVars.color.rgb;
        }

        public override void SpitOutOfShortCut(IntVector2 pos, Room newRoom, bool spitOutAllSticks)
        {
            base.SpitOutOfShortCut(pos, newRoom, spitOutAllSticks);
            Vector2 a = Custom.IntVector2ToVector2(newRoom.ShorcutEntranceHoleDirection(pos));
            for (int i = 0; i < bodyChunks.Length; i++)
            {
                bodyChunks[i].pos = newRoom.MiddleOfTile(pos) - a * (-1.5f + (float)i) * 15f;
                bodyChunks[i].lastPos = newRoom.MiddleOfTile(pos);
                bodyChunks[i].vel = a * 2f;
            }
            if (graphicsModule != null)
            {
                graphicsModule.Reset();
            }
        }

        public override void InitiateGraphicsModule()
        {
            if (graphicsModule == null)
            {
                graphicsModule = new CandleGraphics(this);
            }
            graphicsModule.Reset();
        }

        public void InitiateJump(BodyChunk target)
        {
            if (this.jumpCharging > 0f || this.jumping)
            {
                return;
            }
            this.jumpCharging = 0.01f;
            this.jumpAtChunk = target;
            this.jumpAtPos = Vector2.zero;
            this.room.PlaySound(SoundID.Slugcat_Super_Jump, this.bodyChunks[0]);
        }

        public void InitiateJump(Vector2 target)
        {
            if (this.jumpCharging > 0f || this.jumping)
            {
                return;
            }
            this.jumpCharging = 0.01f;
            this.jumpAtChunk = null;
            this.jumpAtPos = target;
            this.room.PlaySound(SoundID.Slugcat_Super_Jump, this.bodyChunks[0]);
        }

        private void Jump(Vector2 dir, BodyChunk attacking)
        {
            float d = Custom.LerpMap(dir.y, -1f, 1f, 0.7f, 1.2f, 1.1f);
            this.footingCounter = 0;
            base.mainBodyChunk.vel *= 0.5f;
            base.bodyChunks[1].vel *= 0.5f;
            base.mainBodyChunk.vel += dir * 21f * d;
            base.bodyChunks[1].vel += dir * 16f * d;
            this.jumpCharging = 0f;
            this.jumping = true;
            if (attacking != null && attacking.owner is Creature)
            {
                this.AI.targetCreature = (attacking.owner as Creature).abstractCreature;
            }
        }

        internal bool TryToGrabPrey(PhysicalObject prey)
        {
            BodyChunk chunk = null;
            float a = float.MaxValue;
            for (int i = 0; i < prey.bodyChunks.Length; i++)
            {
                if (Custom.DistLess(base.mainBodyChunk.pos, prey.bodyChunks[i].pos, Mathf.Max(a, prey.bodyChunks[i].rad + base.mainBodyChunk.rad + 3f)))
                {
                    a = Vector2.Distance(base.mainBodyChunk.pos, prey.bodyChunks[i].pos);
                    chunk = prey.bodyChunks[i];
                }
            }
            if (chunk != null)
            {
                return Grab(prey, 0, chunk.index, Creature.Grasp.Shareability.CanNotShare, 1f, false, true);
            }
            return false;
        }

        public struct IndividualVariations
        {
            public float dominance;
            public int tailLength;

            public HSLColor color;

            public IndividualVariations(float dominance, int tailLength, HSLColor color)
            {
                this.dominance = dominance;
                this.tailLength = tailLength;
                this.color = color;
            }
        }
    }
}
