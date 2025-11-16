using RWCustom;
using System;
using System.Numerics;
using System.Reflection;
using UnityEngine;
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
        }

        public bool Weaponized()
        {
            if (owner.State.alive)
            {
                if (this.wantToGrabChunk != null && this.wantToGrabChunk.owner is Weapon)
                {
                    return true;
                }
                if (this.grabChunk != null && this.grabChunk.owner is Weapon)
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
            if (wantToGrabChunk != null)
            {
                Debug.Log("wantToGrabChunk != null");
                if (wantToGrabChunk.owner.room != owner.room)
                {
                    Debug.Log("wantToGrabChunk not in same room");
                    wantToGrabChunk = null;
                }
                else
                {
                    if (Vector2.Distance(owner.neck.Tip.pos, wantToGrabChunk.pos) < 25)
                    {
                        grabChunk = wantToGrabChunk;
                        if (grabChunk.owner is Player)
                        {
                            owner.room.PlaySound(SoundID.Vulture_Grab_Player, grabChunk.pos);
                        }
                        else if (grabChunk.owner is Weapon)
                        {
                            owner.room.PlaySound(SoundID.Slugcat_Pick_Up_Spear, this.grabChunk.pos);
                            if (grabChunk.owner is Spear)
                            {
                                (grabChunk.owner as Spear).PulledOutOfStuckObject();
                                (grabChunk.owner as Spear).PickedUp(owner);
                                (grabChunk.owner as Spear).ChangeMode(Weapon.Mode.Free);
                            }
                        }
                        else
                        {
                            owner.room.PlaySound(SoundID.Vulture_Grab_NPC, grabChunk.pos);
                        }
                        wantToGrabChunk = null;
                        grabChunk.pos = owner.Head().pos + Custom.DirVec(owner.neck.Tip.pos, owner.Head().pos) * 5f;
                        grabChunk.vel *= 0f;
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
            else if (grabChunk != null && grabChunk.owner.room != owner.room)
            {
                Debug.Log("grabChunk != null && grabChunk.owner.room != owner.room");
                grabChunk = null;
                owner.neck.floatGrabDest = null;
            }
            else if (grabChunk != null)
            {
                Debug.Log("grabChunk != null");
                grabChunk.owner.AllGraspsLetGoOfThisObject(true);
                wantToGrabChunk = null;
                grabChunk.pos = owner.Head().pos;
                grabChunk.vel *= 0f;
                if (Weaponized())
                {
                    if (grabChunk.owner is Spear)
                    {
                        (grabChunk.owner as Spear).setRotation = new Vector2?(Custom.PerpendicularVector(Custom.DirVec(owner.neck.tChunks[owner.neck.tChunks.Length - 2].lastPos, owner.neck.Tip.lastPos)));
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
                        Vector2 vector2 = vector;
                        if (room == owner.room.abstractRoom.index)
                        {
                            Vector2 pos = owner.neck.Tip.pos;
                            Vector2 a = Custom.DirVec(grabChunk.pos, vector2);
                            if (owner.neck.floatGrabDest == null)
                            {
                                owner.neck.Tip.vel += a * -40f;
                                if (Vector2.Distance(grabChunk.pos, owner.mainBodyChunk.pos) > 80f && Vector2.Distance(grabChunk.pos, vector2) < Vector2.Distance(vector2, owner.mainBodyChunk.pos))
                                {
                                    if (owner.neck.Tip.vel.magnitude > 25f)
                                    {
                                        IntVector2 intVector = IntVector2.FromVector2(a.normalized * 2f);
                                        intVector = IntVector2.ClampAtOne(intVector);
                                        if (intVector.x != 0 || intVector.y != 0)
                                        {
                                            string str = "Buzzard throw weapon ";
                                            PhysicalObject objectOwner = grabChunk.owner;
                                            Debug.Log(str + ((owner != null) ? owner.ToString() : null));
                                            string str2 = "Dir ";
                                            IntVector2 intVector2 = intVector;
                                            Debug.Log(str2 + intVector2.ToString());
                                            (grabChunk.owner as Weapon).Thrown(owner, this.grabChunk.pos, new Vector2?(this.grabChunk.pos - intVector.ToVector2() * 15f), intVector, 1f, true);
                                            this.grabChunk = null;
                                            this.wantToGrabChunk = null;
                                        }
                                    }
                                    else
                                    {
                                        owner.neck.floatGrabDest = null;
                                    }
                                }
                            }
                            else if ((Vector2.Distance(grabChunk.pos, owner.mainBodyChunk.pos) > 80f && owner.room.RayTraceTilesForTerrain(IntVector2.FromVector2(pos / 20f).x, IntVector2.FromVector2(pos / 20f).y, IntVector2.FromVector2(grabChunk.pos / 20f).x, IntVector2.FromVector2(grabChunk.pos / 20f).y) && Vector2.Distance(grabChunk.pos, vector2) > Vector2.Distance(vector2, grabChunk.pos)) || Vector2.Distance(grabChunk.pos, vector2) < 10f)
                            {
                                owner.neck.floatGrabDest = null;
                                owner.neck.tChunks[owner.neck.tChunks.Length - 1].vel += a * 80f;
                                owner.neck.tChunks[owner.neck.tChunks.Length - 2].vel += a * 80f;
                                owner.neck.tChunks[owner.neck.tChunks.Length - 3].vel += a * 80f;
                                owner.neck.tChunks[owner.neck.tChunks.Length - 4].vel += a * 80f;
                                owner.neck.tChunks[owner.neck.tChunks.Length - 5].vel += a * 80f;
                            }
                            else
                            {
                                owner.neck.floatGrabDest = new Vector2?(owner.mainBodyChunk.pos + a * -80f);
                            }
                        }
                        else
                        {
                            owner.neck.floatGrabDest = null;
                        }
                    }
                    else if (Random.value < 0.01f && !owner.safariControlled)
                    {
                        grabChunk = null;
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
