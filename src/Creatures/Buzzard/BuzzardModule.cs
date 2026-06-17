using RWCustom;
using System;
using System.Numerics;
using System.Reflection;
using UnityEngine;
using static Watcher.FireSpriteGraphics;
using Vector2 = UnityEngine.Vector2;
using Random = UnityEngine.Random;

namespace Deadlands.Creatures.Buzzard
{
    internal class BuzzardModule
    {
        public Vulture owner;
        public TailSegment[] tail;

        public BodyChunk wantToGrabChunk;
        public BodyChunk grabChunk;

        public float panic;
        public float wantsToThrowSpear;
        public int coudldown =0;

        public VultureGraphics graphics
        {
            get
            {
                return owner.graphicsModule as VultureGraphics;
            }
        }

        public BuzzardAI AI
        {
            get
            {
                return owner.AI as BuzzardAI;
            }
        }

        public BuzzardModule(Vulture vulture)
        {
            owner = vulture;

            grabChunk = owner.Head();
            wantToGrabChunk = owner.Head();
        }

        public bool Weaponized()
        {
            if (owner.State.alive)
            {
                /*if (this.wantToGrabChunk != null && this.wantToGrabChunk.owner is Weapon)
                {
                    return true;
                }*/
                if (owner.grasps[0].grabbedChunk != null && owner.grasps[0].grabbedChunk.owner is Weapon)
                {
                    return true;
                }
                       
            }
            return false;
        }
        /// <summary>
        /// Buzzard Throw Spear code, largely based on Inspector code.
        /// </summary>
        public void Act(Vulture self)
        {
            if (coudldown>=0)
            {
                coudldown--;
            }
            if (AI.focusWepon != null && self.grasps[0] == null && coudldown<=0 && this.AI.preyTracker.MostAttractivePrey != null && !this.AI.preyTracker.MostAttractivePrey.representedCreature.realizedCreature.dead)
            {
                //Debug.Log("wantToGrabChunk != null");
                if (AI.focusWepon.representedItem.realizedObject.room != owner.room)
                {
                    Debug.Log("wantToGrabChunk not in same room");
                    AI.focusWepon = null;
                }
                else
                {
                    
                    if (Custom.DistLess(owner.Head().pos, AI.focusWepon.representedItem.realizedObject.bodyChunks[0].pos, AI.focusWepon.representedItem.realizedObject.bodyChunks[0].rad + 30f) && (Custom.DistLess(owner.Head().pos, AI.focusWepon.representedItem.realizedObject.bodyChunks[0].pos, AI.focusWepon.representedItem.realizedObject.bodyChunks[0].rad + 15f) || owner.room.VisualContact(owner.bodyChunks[0].pos, AI.focusWepon.representedItem.realizedObject.bodyChunks[0].pos)))
                    {
                        Debug.Log("Stean222");
                        owner.Grab(AI.focusWepon.representedItem.realizedObject, 0, 0, Creature.Grasp.Shareability.CanOnlyShareWithNonExclusive, 1f, true, true);
                        grabChunk = AI.focusWepon.representedItem.realizedObject.bodyChunks[0];
                        if (self.grasps[0].grabbedChunk.owner is Player)
                        {
                            owner.room.PlaySound(SoundID.Vulture_Grab_Player, AI.focusWepon.representedItem.realizedObject.bodyChunks[0].pos);
                        }
                        else if (self.grasps[0].grabbedChunk.owner is Weapon)
                        {
                            Debug.Log("Stean333");
                            owner.room.PlaySound(SoundID.Slugcat_Pick_Up_Spear, AI.focusWepon.representedItem.realizedObject.bodyChunks[0].pos);
                            if (self.grasps[0].grabbedChunk.owner is Spear)
                            {
                                (self.grasps[0].grabbedChunk.owner as Spear).PulledOutOfStuckObject();
                                (self.grasps[0].grabbedChunk.owner as Spear).PickedUp(owner);
                                (self.grasps[0].grabbedChunk.owner as Spear).ChangeMode(Weapon.Mode.Free);
                            }
                        }
                        else
                        {
                            owner.room.PlaySound(SoundID.Vulture_Grab_NPC, grabChunk.pos);
                        }
                        wantToGrabChunk = null;
                    }
                    else if (Weaponized() && Vector2.Distance(owner.mainBodyChunk.pos, wantToGrabChunk.pos) > 800f)
                    {
                        Debug.Log("Weaponized() && Vector2.Distance(owner.mainBodyChunk.pos, wantToGrabChunk.pos) > 800f");
                        wantToGrabChunk = null;
                    }
                    else if (!Weaponized() && Vector2.Distance(owner.neck.Tip.pos, wantToGrabChunk.pos) > 1300f)
                    {
                        Debug.Log("!Weaponized() && Vector2.Distance(owner.neck.Tip.pos, wantToGrabChunk.pos) > 1300f");
                        wantToGrabChunk = null;
                    }
                    else if (!owner.safariControlled || panic > 0.5f)
                    {
                        for (int i = 0; i < AI.itemTracker.ItemCount; i++)
                        {
                            ItemTracker.ItemRepresentation rep = AI.itemTracker.GetRep(i);
                            PhysicalObject realizedObject = rep.representedItem.realizedObject;
                            if (realizedObject != null && rep.VisualContact && Vector2.Distance((realizedObject as Weapon).firstChunk.pos, owner.mainBodyChunk.pos) < 400f && Vector2.Distance((realizedObject as Weapon).firstChunk.pos, owner.Head().pos) > 10f && (realizedObject as Weapon).mode != Weapon.Mode.Thrown && Vector2.Distance(owner.Head().pos, (realizedObject as Weapon).firstChunk.pos) < Vector2.Distance(owner.Head().pos, wantToGrabChunk.pos))
                            {
                                Debug.Log("wantToGrabChunk = (realizedObject as Weapon).firstChunk;");
                                wantToGrabChunk = (realizedObject as Weapon).firstChunk;
                            }
                        }
                    }
                }
            }
            else if (self.grasps[0].grabbedChunk != null && self.grasps[0].grabbedChunk.owner.room != owner.room)
            {
                Debug.Log("self.grasps[0].grabbedChunk != null && self.grasps[0].grabbedChunk.owner.room != owner.room");
                self.grasps[0] = null;
                owner.neck.floatGrabDest = null;
            }
            else if(self.grasps[0].grabbedChunk.owner is Weapon)
            {
                Debug.Log("self.grasps[0].grabbedChunk != null");
                wantToGrabChunk = null;
                if (Weaponized())
                {
                    //Debug.Log("Weaponized is active");
                    if (self.grasps[0].grabbedChunk.owner is Spear)
                    {
                        (self.grasps[0].grabbedChunk.owner as Spear).setRotation = new Vector2?(-Custom.DirVec(owner.neck.tChunks[owner.neck.tChunks.Length - 2].lastPos, owner.neck.Tip.lastPos));
                    }
                    Creature creature = null;
                    Vector2 vector = Vector2.zero;
                    int room = 0;
                    if (!owner.safariControlled && this.AI.preyTracker.MostAttractivePrey != null)
                    {
                        creature = this.AI.preyTracker.MostAttractivePrey.representedCreature.realizedCreature;
                        vector = this.AI.preyTracker.MostAttractivePrey.lastSeenCoord.Tile.ToVector2() * 20f;
                        room = this.AI.preyTracker.MostAttractivePrey.lastSeenCoord.room;
                    }
                    if (creature != null)
                    {
                        if (creature.dead)
                        {
                            self.grasps[0] = null;
                        }
                        if (room == owner.room.abstractRoom.index)
                        {
                            Vector2 pos = owner.neck.Tip.pos;
                                if ((!self.AirBorne || Random.value < 0.016666668f) && self.snapFrames == 0 && !self.safariControlled && Custom.DistLess(self.mainBodyChunk.pos, creature.abstractCreature.realizedCreature.bodyChunks[Random.Range(0, creature.abstractCreature.realizedCreature.bodyChunks.Length)].pos, 520f) && self.room.VisualContact(self.bodyChunks[4].pos, creature.abstractCreature.realizedCreature.bodyChunks[Random.Range(0, creature.abstractCreature.realizedCreature.bodyChunks.Length)].pos))
                                {
                                    Debug.Log("Steap4");
                                    owner.Snap(creature.abstractCreature.realizedCreature.bodyChunks[Random.Range(0, creature.abstractCreature.realizedCreature.bodyChunks.Length)]);
                                    
                                }
                               
                                
                          
                        }
                        else
                        {
                            owner.neck.floatGrabDest = null;
                        }
                    }
                    else if (Random.value < 0.01f && !owner.safariControlled)
                    {
                        self.grasps[0] = null;
                    }

                    Vector2 vector2 = vector;
                    Vector2 a = Custom.DirVec(self.grasps[0].grabbedChunk.pos, vector2);
                    if (self.snapFrames <= 7 && self.snapFrames > 0)
                    {
                        Debug.Log("Steap5");
                        IntVector2 intVector = IntVector2.FromVector2(a.normalized * 2f);
                        if (intVector.x != 0 || intVector.y != 0)
                        {
                            string str = "Buzzard throw weapon ";
                        PhysicalObject objectOwner = self.grasps[0].grabbedChunk.owner;
                        Debug.Log(str + ((owner != null) ? owner.ToString() : null));
                        string str2 = "Dir ";
                        IntVector2 intVector2 = intVector;
                        Debug.Log(str2 + intVector2.ToString());
                        (self.grasps[0].grabbedChunk.owner as Weapon).Shoot(owner, self.grasps[0].grabbedChunk.pos + intVector.ToVector2() * 25f, a.normalized, 1f, owner.evenUpdate);
                        Debug.Log((self.grasps[0].grabbedChunk.owner as Weapon).firstChunk.vel.magnitude);
                        self.grasps[0] = null;
                        this.wantToGrabChunk = null;
                        coudldown = 10;
                        }
                    }
                }
            }
        }

        public void InitiateGraphics(VultureGraphics graphics)
        {
            tail = new TailSegment[7];
            for (int j = 0; j < 7; j++)
            {
                float num = 14f;
                num *= ((float)(7 - j) / (float)7 * 4f + 1f) / 5f;
                float num2 = (((j > 0) ? 14f : 28f) + num) / 2f;
                num2 *= 1.4f;
                this.tail[j] = new TailSegment(graphics, num, num2, (j > 0) ? this.tail[j - 1] : null, 0.85f, 1f, 0.4f, false);
            }
        }

        public void DrawSprites(VultureGraphics graphics, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            Vector2 vector = Vector2.Lerp(owner.bodyChunks[1].lastPos, owner.bodyChunks[1].pos, timeStacker);
            Vector2 vector2 = Vector2.Lerp(owner.bodyChunks[0].lastPos, owner.bodyChunks[0].pos, timeStacker);
            Vector2 vector4 = Vector2.Lerp(vector, vector2, timeStacker);
            int index = 15;
            float d2 = 12f;
            for (int i = 0; i < tail.Length; i++)
            {
                Vector2 vector5 = Vector2.Lerp(tail[i].lastPos, tail[i].pos, timeStacker);
                Vector2 normalized = (vector5 - vector4).normalized;
                Vector2 a = Custom.PerpendicularVector(normalized);
                float d3 = Vector2.Distance(vector5, vector4) / 5f;
                if (i == 0)
                {
                    d3 = 0;
                }
                (sLeaser.sprites[index] as TriangleMesh).MoveVertice(i * 4, vector4 - a * (tail[i].StretchedRad * 0.5f) + normalized * d3 - camPos);
                (sLeaser.sprites[index] as TriangleMesh).MoveVertice(i * 4 + 1, vector4 + a * (tail[i].StretchedRad * 0.5f) + normalized * d3 - camPos);
                if (i < tail.Length - 1)
                {
                    (sLeaser.sprites[index] as TriangleMesh).MoveVertice(i * 4 + 2, vector5 - a * tail[i].StretchedRad - normalized * d3 - camPos);
                    (sLeaser.sprites[index] as TriangleMesh).MoveVertice(i * 4 + 3, vector5 + a * tail[i].StretchedRad - normalized * d3 - camPos);
                }
                else
                {
                    (sLeaser.sprites[index] as TriangleMesh).MoveVertice(i * 4 + 2, vector5 - camPos);
                }
                d2 = tail[i].StretchedRad;
                vector4 = vector5;
            }
        }

        public void UpdateGraphics(VultureGraphics self)
        {
            tail[0].connectedPoint = owner.mainBodyChunk.pos;
            Vector2 vector = owner.Head().pos;
            for (int i = 0; i < tail.Length; i++)
            {
                tail[i].Update();
                if (owner.room.PointSubmerged(tail[i].pos))
                {
                    tail[i].vel *= 0.8f;
                }
                else
                {
                    tail[i].vel += Custom.DirVec(vector, tail[i].pos) * 250f * Mathf.Pow(0.4f, (float)i) / Vector2.Distance(vector, tail[i].pos);
                }
            }
        }
    }
}
