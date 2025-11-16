using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RWCustom;
using System.Numerics;
using System.Reflection;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;
using Random = UnityEngine.Random;


namespace Deadlands.Creatures.Iguana
{
    internal class IguanaModule
    {
        
            public Lizard owner;

        public TailSegment[] tail;

        public BodyChunk wantToGrabChunk;
            public BodyChunk grabChunk;


            public bool Jerbo;

            public LizardGraphics graphics
            {
                get
                {
                    return owner.graphicsModule as LizardGraphics;
                }
            }

            public IguanaAI AI
            {
                get
                {
                    return owner.AI as IguanaAI;
                }
            }

            public IguanaModule(Lizard lizard)
            {
                owner = lizard;
            }

         public void InitiateGraphics(LizardGraphics graphics)
        {
            
                tail = new TailSegment[10];
                for (int j = 0; j < 7; j++)
                {
                    float num = 14f;
                    num *= ((float)(7 - j) / (float)7 * 4f + 1f) / 5f;
                    float num2 = (((j > 0) ? 14f : 28f) + num) / 2f;
                    num2 *= 1.4f;
                    this.tail[j] = new TailSegment(graphics, num, num2, (j > 0) ? this.tail[j - 1] : null, 0.85f, 1f, 0.4f, false);
                }
            
        }

        public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            
            
        }
        public void BiteVegis(PhysicalObject chunk, Lizard self)
        {
            
            if (self.grasps[0] != null && self.grabbedBy.Count == 0)
            {
                return;
            }
            
            self.biteControlReset = false;
            self.JawOpen = 0f;
            self.lastJawOpen = 0f;
          
            
            bool flag = false;
            if ((chunk is DangleFruit) || (Random.value < 0.5f && chunk.TotalMass < self.TotalMass * 1.2f) || (!(chunk is PlayerCarryableItem) && chunk.TotalMass < self.TotalMass * 1.2f))
            {
                flag = self.Grab(chunk, 0, 0, Creature.Grasp.Shareability.CanNotShare, 1f, true, true);
                self.room.PlaySound(SoundID.Jet_Fish_Grab_NPC, self.mainBodyChunk);
            }

           
            self.room.PlaySound(SoundID.Lizard_Jaws_Shut_Miss_Creature, self.mainBodyChunk);
            for (int j = chunk.grabbedBy.Count - 1; j >= 0; j--)
            {
                if (chunk.grabbedBy[j].grabber is Lizard)
                {
                    chunk.grabbedBy[j].Release();
                }
            }
        }
        public void AttemptBiteVegis(PlayerCarryableItem creature, Lizard self)
        {
            if (self.grasps[0] != null || !self.Consious)
            {
                return;
            }
            if (self.JawReadyForBite)
            {
                bool flag = false;
                if (Random.value < self.lizardParams.biteChance && creature != null)
                {
                    PhysicalObject chunk = creature;
                    int i = 0;
                    while (i == 1)
                    {
                        
                        if (Custom.DistLess(self.mainBodyChunk.pos + Custom.DirVec(self.bodyChunks[1].pos, self.mainBodyChunk.pos) * self.lizardParams.biteInFront, chunk.bodyChunks[1].pos, (ModManager.MMF ? Mathf.Max(8f, chunk.bodyChunks[1].rad) : chunk.bodyChunks[1].rad) + self.lizardParams.biteRadBonus))
                        {
                            
                            flag = true;
                            BiteVegis(chunk, self);
                            break;
                        }
                        else
                        {
                            i++;
                        }
                    }
                }
                else if (creature == null)
                {
                    BiteVegis(null, self);
                }
                if (self.LegsGripping > 0)
                {
                    if (flag)
                    {
                        for (int j = 0; j < 3; j++)
                        {
                            self.bodyChunks[j].vel += Custom.DegToVec(Random.value * 360f) * 7f;
                        }
                        return;
                    }
                    if (creature != null && (self.tongue == null || !self.tongue.Out))
                    {
                        self.mainBodyChunk.vel += Custom.DirVec(self.mainBodyChunk.pos, creature.bodyChunks[0].pos) * 3f * self.lizardParams.biteHomingSpeed;
                        self.bodyChunks[1].vel -= Custom.DirVec(self.mainBodyChunk.pos, creature.bodyChunks[0].pos) * self.lizardParams.biteHomingSpeed;
                        self.bodyChunks[2].vel -= Custom.DirVec(self.mainBodyChunk.pos, creature.bodyChunks[0].pos) * self.lizardParams.biteHomingSpeed;
                    }
                }
                return;
            }
            if (self.safariControlled)
            {
                self.biteDelay = 0;
                self.JawOpen += 0.2f;
                return;
            }
            self.JawOpen += 0.05f;
        }

        public void Collide(On.Lizard.orig_Collide orig, Lizard self, PhysicalObject otherObject, int myChunk, int otherChunk)
        {
            orig(self, otherObject, myChunk, otherChunk);
            if (self.Template.type == DLCreature.Iguana)
            {

                
                if (self.Consious && self.grasps[0] == null  && AI.WantToEatFruts(otherObject) )
                {
                    if (self.animation == Lizard.Animation.Lounge && myChunk == 0)
                    {
                        self.Bite(otherObject.bodyChunks[otherChunk]);
                    }
                    else if (!self.safariControlled)
                    {
                        AttemptBiteVegis(null, self);
                    }
                }
                else if (!(otherObject is Lizard) && otherObject.bodyChunks[otherChunk].pos.y < self.bodyChunks[myChunk].pos.y )
                {
                    BodyChunk bodyChunk = otherObject.bodyChunks[otherChunk];
                    bodyChunk.vel.y = bodyChunk.vel.y -  otherObject.bodyChunks[otherChunk].mass;
                    BodyChunk bodyChunk2 = self.bodyChunks[myChunk];
                    bodyChunk2.vel.y = bodyChunk2.vel.y +  2f;
                    int num2 = 30;
                    if (otherObject is Creature)
                    {
                        SocialMemory.Relationship relationship = self.abstractCreature.state.socialMemory.GetRelationship((otherObject as Creature).abstractCreature.ID);
                        if (relationship != null)
                        {
                            if (relationship.like > -0.5f)
                            {
                                relationship.like = Mathf.Lerp(relationship.like, 0f, 0.001f);
                            }
                            if (relationship.like >= 0f)
                            {
                                num2 = 10 + (int)(20f * Mathf.InverseLerp(1f, 0f, relationship.like));
                            }
                            else
                            {
                                num2 = 30 + (int)(220f * Mathf.InverseLerp(0f, -1f, relationship.like));
                            }
                        }
                    }
                    
                }
                
            }
        }





    }
}
