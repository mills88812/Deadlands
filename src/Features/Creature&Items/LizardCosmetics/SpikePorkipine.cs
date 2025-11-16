using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using MoreSlugcats;
using RWCustom;
using Random = UnityEngine.Random;

namespace LizardCosmetics
{
    
       

    public class SpikePorkipine : Template
    {
        public GenericBodyPart[,] scales;

        public int graphic;

        public float scaleLength;

        public float graphicLenght;

        public float frontDir;

        public float backDir;

        public float sturdy;

        public float posSqueeze;

        public int ScaleSprite(int s, int i)
        {
            return startSprite + s * scales.GetLength(1) + i;
        }

        public SpikePorkipine(LizardGraphics lGraphics, int startSprite)
            : base(lGraphics, startSprite)
        {
            spritesOverlap = SpritesOverlap.BehindHead;
            scales = new GenericBodyPart[6, (Random.value < 0.2f) ? 8 : 6];
            graphic = ((!(Random.value < 0.4f)) ? Random.Range(0, 0) : 2);
            graphicLenght = Futile.atlasManager.GetElementWithName("LizardScaleA" + graphic).sourcePixelSize.y;
            sturdy = Random.value;
            posSqueeze = Random.value;
            scaleLength = Mathf.Lerp(5f, 40f, Mathf.Pow((Random.value < 0.2f) ? 0.9f : 0.7f, 0.75f + 1.25f * sturdy));
            frontDir = Mathf.Lerp(-0.1f, 0.2f, Random.value);
            backDir = Mathf.Lerp(Mathf.Max(0f, frontDir), frontDir + (float)scales.GetLength(1) * 0.2f, Random.value);
            for (int i = 0; i < scales.GetLength(0); i++)
            {
                for (int j = 0; j < scales.GetLength(1); j++)
                {
                    scales[i, j] = new GenericBodyPart(lGraphics, 2f, 0.5f, Mathf.Lerp(0.8f, 0.999f, sturdy), lGraphics.lizard.bodyChunks[1]);
                }
            }

            numberOfSprites = scales.GetLength(0) * scales.GetLength(1);
        }

        public override void Reset()
        {
            base.Reset();
            for (int i = 0; i < scales.GetLength(0); i++)
            {
                for (int j = 0; j < scales.GetLength(1); j++)
                {
                    scales[i, j].pos = scales[i, j].connection.pos;
                    scales[i, j].lastPos = scales[i, j].connection.pos;
                    scales[i, j].vel *= 0f;
                }
            }
        }

        public override void Update()
        {
            for (int i = 0; i < scales.GetLength(1); i++)
            {
                float num = Custom.LerpMap(i, 0f, scales.GetLength(1) - 1, frontDir, backDir);
                LizardGraphics.LizardSpineData lizardSpineData = lGraphics.SpinePosition(0.025f + (0.025f + 0.15f * (float)i) * posSqueeze, 1f);
                float f = Mathf.Lerp(lGraphics.headDepthRotation, lizardSpineData.depthRotation, 0.3f + 0.2f * (float)i);
                for (int j = 0; j < scales.GetLength(0); j++)
                {
                    Vector2 vector = lizardSpineData.pos + lizardSpineData.perp * ((j == 0) ? (-1f) : 1f) * lizardSpineData.rad * (1f - Mathf.Abs(f));
                    Vector2 vector2 = lizardSpineData.perp * ((j == 0) ? (-1f) : 1f) * (1f - Mathf.Abs(f));
                    vector2 = Vector3.Slerp(vector2, lizardSpineData.dir * num, Mathf.Abs(num));
                    vector2 = Vector3.Slerp(vector2, lizardSpineData.perp * Mathf.Sign(f), Mathf.Abs(f) * 0.5f);
                    Vector2 vector3 = vector + vector2 * scaleLength * 1.5f;
                    scales[j, i].Update();
                    scales[j, i].ConnectToPoint(vector, scaleLength * ((i > 1) ? 0.6f : 1f), push: false, 0f, lGraphics.lizard.bodyChunks[1].vel, 0.1f + 0.2f * sturdy, 0f);
                    scales[j, i].vel += (vector3 - scales[j, i].pos) * Mathf.Lerp(0.1f, 0.3f, sturdy);
                    scales[j, i].pos += (vector3 - scales[j, i].pos) * 0.6f * Mathf.Pow(sturdy, 3f);
                }
            }

            if (!(lGraphics.lizard.animation == Lizard.Animation.PrepareToJump) || !lGraphics.lizard.Consious)
            {
                return;
            }

            for (int k = 0; k < scales.GetLength(0); k++)
            {
                for (int l = 0; l < scales.GetLength(1); l++)
                {
                    scales[k, l].vel += Custom.RNV() * 3f * Random.value + Custom.DirVec(lGraphics.lizard.bodyChunks[1].pos, lGraphics.lizard.bodyChunks[0].pos) * Random.value * 5f;
                    scales[k, l].pos += Custom.RNV() * 3f * 1f;
                }
            }
        }

        public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            for (int i = 0; i < scales.GetLength(0); i++)
            {
                for (int j = 0; j < scales.GetLength(1); j++)
                {
                    sLeaser.sprites[ScaleSprite(i, j)] = new FSprite("LizardScaleA" + graphic);
                    sLeaser.sprites[ScaleSprite(i, j)].anchorY = 0f;
                    sLeaser.sprites[ScaleSprite(i, j)].scaleX = ((i == 0) ? (-1f) : 1f);
                }
            }
        }

        public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            for (int i = 0; i < scales.GetLength(1); i++)
            {
                LizardGraphics.LizardSpineData lizardSpineData = lGraphics.SpinePosition(0.025f + (0.025f + 0.15f * (float)i) * posSqueeze, timeStacker);
                for (int j = 0; j < scales.GetLength(0); j++)
                {
                    Vector2 vector = lizardSpineData.pos + lizardSpineData.perp * ((j == 0) ? (-1f) : 1f) * lizardSpineData.rad * (1f - Mathf.Abs(lizardSpineData.depthRotation));
                    Vector2 vector2 = Vector2.Lerp(scales[j, i].lastPos, scales[j, i].pos, timeStacker);
                    sLeaser.sprites[ScaleSprite(j, i)].x = vector.x - camPos.x;
                    sLeaser.sprites[ScaleSprite(j, i)].y = vector.y - camPos.y;
                    sLeaser.sprites[ScaleSprite(j, i)].scaleY = Vector2.Distance(vector, vector2) / graphicLenght;
                    sLeaser.sprites[ScaleSprite(j, i)].rotation = Custom.AimFromOneVectorToAnother(vector, vector2);
                }
            }
        }

        public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            for (int i = 0; i < numberOfSprites; i++)
            {
                

                
                    sLeaser.sprites[startSprite + i].color = palette.blackColor;
                
            }
        }
    }
}

