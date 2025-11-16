using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System;
using UnityEngine;
using Random = UnityEngine.Random;
using RWCustom;

namespace LizardCosmetics;

public class TailTipGlow : Template
{
    public int bumps;

    public float spineLength;

    public float sizeSkewExponent;

    public float sizeRangeMin;

    public float sizeRangeMax;

    public bool coloredHawk;


   
    internal Room room;

    internal TailSegment tailtip;

    private readonly LightSource[] lightSources = new LightSource[3];
    public TailTipGlow(LizardGraphics lGraphics, int startSprite)
        : base(lGraphics, startSprite)
    {
        coloredHawk = UnityEngine.Random.value < 0.5f;
        spritesOverlap = SpritesOverlap.BehindHead;
        float num;
        if (coloredHawk)
        {
            num = Mathf.Lerp(3f, 8f, Mathf.Pow(UnityEngine.Random.value, 0.7f));
            spineLength = Mathf.Lerp(1f, 1f, UnityEngine.Random.value) * lGraphics.tailLength;
            sizeRangeMin = Mathf.Lerp(1f, 1f, UnityEngine.Random.value);
            sizeRangeMax = Mathf.Lerp(sizeRangeMin, 0.35f, Mathf.Pow(UnityEngine.Random.value, 0.5f));
        }
        else
        {
            num = Mathf.Lerp(6f, 12f, Mathf.Pow(UnityEngine.Random.value, 0.5f));
            spineLength = Mathf.Lerp(1f, 1f, UnityEngine.Random.value) * lGraphics.tailLength;
            sizeRangeMin = Mathf.Lerp(1f, 1f, Mathf.Pow(UnityEngine.Random.value, 0.5f));
            sizeRangeMax = Mathf.Lerp(sizeRangeMin, 1f, UnityEngine.Random.value);
        }

        sizeSkewExponent = Mathf.Lerp(0.1f, 0.7f, UnityEngine.Random.value);
        bumps = (int)(spineLength / 4);
        numberOfSprites = 1;
        
        
    }

    public override void Update()
    {
        
        
    }

    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        for (int num = startSprite + numberOfSprites - 1; num >= startSprite; num--)
        {
            float num2 = Mathf.InverseLerp(startSprite, startSprite + numberOfSprites - 1, num);
            sLeaser.sprites[num] = new FSprite("DangleFruit0A");
            sLeaser.sprites[num].scale = Mathf.Lerp(sizeRangeMin, sizeRangeMax, Mathf.Lerp(Mathf.Sin(Mathf.Pow(num2, sizeSkewExponent) * (float)Math.PI), 1f, (num2 < 0.5f) ? 1f : 0f));

            sLeaser.sprites[num].anchorY = 0.15f;
        }
    }

    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        for (int num = startSprite + numberOfSprites - 1; num >= startSprite; num--)
        {
            float num2 = Mathf.Lerp(startSprite, startSprite + numberOfSprites - 1, num);
            float num3 = Mathf.Lerp(0.05f, spineLength / lGraphics.tailLength, num2);
            LizardGraphics.LizardSpineData lizardSpineData = lGraphics.SpinePosition(num3, timeStacker);
            Vector2 vector = lizardSpineData.pos;
            Vector2 a2 = Custom.PerpendicularVector(vector);
            Vector2 vector2 = Custom.DegToVec(num2 + ( -40f));
            Vector2 vector3 = lizardSpineData.pos + a2 *  vector2.x;
            sLeaser.sprites[num].x = vector3.x - camPos.x;
            sLeaser.sprites[num].y = vector3.y - camPos.y;
            sLeaser.sprites[num].rotation = Custom.VecToDeg(Vector2.Lerp(lizardSpineData.perp * lizardSpineData.depthRotation, lizardSpineData.dir * (float)((num == 1) ? -1 : 1), num2));


            if (lGraphics.lizard.animation == Lizard.Animation.ThreatReSpotted || lGraphics.lizard.Stunned || lGraphics.lizard.animation == Lizard.Animation.ThreatSpotted)
                {

                    sLeaser.sprites[num].color = new Color(Random.value, Random.value, Random.value);

                }
                else
                    sLeaser.sprites[num].color = lGraphics.lizard.effectColor;
               
            
        }
    }

    public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        if (!coloredHawk)
        {
            for (int i = startSprite; i < startSprite + numberOfSprites; i++)
            {
                float f = Mathf.Lerp(0.05f, spineLength / lGraphics.tailLength, Mathf.InverseLerp(startSprite, startSprite + numberOfSprites - 1, i));
                sLeaser.sprites[i].color = lGraphics.BodyColor(f);
            }
        }
    }
    

}