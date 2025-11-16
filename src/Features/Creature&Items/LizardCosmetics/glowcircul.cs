using System;
using RWCustom;
using UnityEngine;
using Random = UnityEngine.Random;

namespace LizardCosmetics
{
    
        public class glowcircul : Template
        {
            public int RingSprite(int ring, int side, int part)
            {
                return this.startSprite + part * 4 + side * 2 + ring;
            }

            public glowcircul(LizardGraphics lGraphics, int startSprite) : base(lGraphics, startSprite)
            {
                this.spritesOverlap = Template.SpritesOverlap.BehindHead;
                this.numberOfSprites = 8;
            }

            public override void Update()
            {
            }

            public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
            {
                for (int i = 0; i < 2; i++)
                {
                    for (int j = 0; j < 2; j++)
                    {
                        for (int k = 0; k < 2; k++)
                        {
                            sLeaser.sprites[this.RingSprite(i, j, k)] = new FSprite("Circle20", true);
                        }
                    }
                }
            }

            public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
            {
                float b = Mathf.Lerp(this.lGraphics.lastDepthRotation, this.lGraphics.depthRotation, timeStacker);
                float a = Mathf.Lerp(this.lGraphics.lastHeadDepthRotation, this.lGraphics.headDepthRotation, timeStacker);
                Color color = this.lGraphics.HeadColor(timeStacker);
                float num = 1f;
                if (this.lGraphics.lizard.animation == Lizard.Animation.ThreatReSpotted || this.lGraphics.lizard.animation == Lizard.Animation.PreyReSpotted || this.lGraphics.lizard.animation == Lizard.Animation.PreySpotted || this.lGraphics.lizard.animation == Lizard.Animation.FightingStance || this.lGraphics.lizard.animation == Lizard.Animation.ThreatSpotted)
                {
                    num = 0.5f + 0.5f * Mathf.Lerp((float)this.lGraphics.lizard.timeToRemainInAnimation, 0f, (float)this.lGraphics.lizard.timeInAnimation);
                    color = Color.Lerp(this.lGraphics.HeadColor(timeStacker), Color.Lerp(Color.white, this.lGraphics.effectColor, num), Random.value);
                }
                for (int i = 0; i < 2; i++)
                {
                    float s = 0.06f + 0.12f * (float)i;
                    LizardGraphics.LizardSpineData lizardSpineData = this.lGraphics.SpinePosition(s, timeStacker);
                    Vector2 vector = lizardSpineData.dir;
                    Vector2 pos = lizardSpineData.pos;
                    if (i == 0)
                    {
                        vector = (vector - Custom.DirVec(Vector2.Lerp(this.lGraphics.drawPositions[0, 1], this.lGraphics.drawPositions[0, 0], timeStacker), Vector2.Lerp(this.lGraphics.head.lastPos, this.lGraphics.head.pos, timeStacker))).normalized;
                    }
                    Vector2 a2 = Custom.PerpendicularVector(vector);
                    float num2 = 50f * Mathf.Lerp(a, b, (i == 0) ? 0.25f : 0.5f);
                    for (int j = 0; j < 2; j++)
                    {
                        Vector2 vector2 = Custom.DegToVec(num2 + (((float)j == 0f) ? -40f : 40f));
                        Vector2 vector3 = pos + a2 * lizardSpineData.rad * vector2.x;
                        Vector2 vector4 = vector;
                        if (i == 0)
                        {
                            vector4 = (vector4 - 2f * Custom.DirVec(vector3, Vector2.Lerp(this.lGraphics.head.lastPos, this.lGraphics.head.pos, timeStacker)) * Mathf.Abs(vector2.y)).normalized;
                        }
                        else
                        {
                            vector4 = (vector4 + 2f * Custom.DirVec(vector3, Vector2.Lerp(this.lGraphics.tail[0].lastPos, this.lGraphics.tail[0].pos, timeStacker)) * Mathf.Abs(vector2.y)).normalized;
                        }
                        
                        vector3 = pos + a2 * (lizardSpineData.rad + 2f * Mathf.Pow(Mathf.Clamp01(Mathf.Abs(vector2.x) * Mathf.Abs(vector2.y)), 0.5f)) * vector2.x;
                        vector3 -= vector4 * (1f - num) * 4f;
                        sLeaser.sprites[this.RingSprite(i, j, 1)].x = vector3.x - camPos.x;
                        sLeaser.sprites[this.RingSprite(i, j, 1)].y = vector3.y - camPos.y;
                        sLeaser.sprites[this.RingSprite(i, j, 1)].rotation = Custom.VecToDeg(vector4);
                        float t = Mathf.Pow(Mathf.Clamp01(Mathf.Abs(vector2.x)), 2f);
                        sLeaser.sprites[this.RingSprite(i, j, 1)].scaleX = ((vector2.y > 0f) ? (Mathf.Lerp(0.45f, 0f, t) * num) : 0f);
                        sLeaser.sprites[this.RingSprite(i, j, 1)].scaleY = 0.55f * num; 
                        sLeaser.sprites[this.RingSprite(i, j, 1)].color = color;
                }
                }
            }

            public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
            {
                for (int i = 0; i < 2; i++)
                {
                    for (int j = 0; j < 2; j++)
                    {
                        sLeaser.sprites[this.RingSprite(i, j, 1)].color = new Color(1f, 1f, 1f);
                    }
                }
            }
        }
    
}
