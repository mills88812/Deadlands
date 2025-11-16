using RWCustom;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Deadlands.Creatures.CandleMouse
{
    internal class FlashTail : BodyPart
    {
        public CandleMouse mouse { get { return (owner as CandleGraphics).mouse as CandleMouse; } }

        public CandleGraphics graphics;
        public TailSegment[] tail;
        public LightSource light;

        public Vector2 bulbPos;

        public FlashTail(CandleGraphics ow) : base(ow)
        {
            this.graphics = ow;
            this.tail = new TailSegment[this.mouse.iVars.tailLength];
            for (int i = 0; i < tail.Length; i++)
            {
                this.tail[i] = new TailSegment(ow, 
                    Mathf.Lerp(2f, 0.5f, 0.5f),
                    8f, (i == 0) ? null : this.tail[i],
                    0.6f, 0.8f, 0.4f, false);
            }
        }

        public void TailUpdate()
        {
            Vector2 vector = this.mouse.bodyChunks[1].pos + Custom.DirVec(this.mouse.mainBodyChunk.pos, this.mouse.bodyChunks[1].pos) * 8f;
            Vector2 vector2 = vector;
            this.tail[0].connectedPoint = vector;
            for (int i = 0; i < tail.Length; i++)
            {
                var rad = this.tail[i].rad; // Should fix terrain clipping
                this.tail[i].rad += 1;
                this.tail[i].Update();
                this.tail[i].rad = rad;
                if (this.mouse.Consious)
                {
                    float inv = Mathf.InverseLerp(0f, this.tail.Length - 1, i);
                    Vector2 a = Custom.PerpendicularVector(vector2) * Mathf.Lerp(1f, -1f, graphics.backToCam);
                    //this.tail[i].pos += Custom.DegToVec(Random.value * 360f) * 2f * Random.value;
                    if (!Custom.DistLess(this.tail[i].pos, this.mouse.bodyChunks[1].pos, 9f * (i + 1)))
                    {
                        this.tail[i].pos = this.mouse.bodyChunks[1].pos + Custom.DirVec(this.mouse.bodyChunks[1].pos, this.tail[i].pos) * 8f * (i + 1f);
                    }
                    if (this.mouse.room.PointSubmerged(this.tail[i].pos))
                    {
                        this.tail[i].vel *= 0.4f;
                    } else
                    {
                        if (i > 2)
                        {
                            vector2 = this.tail[i - 1].pos;
                        }
                        this.tail[i].vel += Custom.DirVec(a, this.tail[i].pos) * Mathf.Lerp(0.1f, 1.4f, inv);
                    }
                }
                else
                {
                    TailSegment tailSegment = this.tail[i];
                    tailSegment.vel.y = tailSegment.vel.y - 0.6f;
                }
                //this.tail[i].PushOutOfTerrain(this.mouse.room, this.mouse.bodyChunks[1].pos);
            }
            this.bulbPos = this.tail[tail.Length - 1].pos;
            if (this.light != null)
            {
                this.light.setPos = this.bulbPos;
                if (this.mouse.room.Darkness(this.bulbPos) == 0f)
                {
                    this.light.Destroy();
                }
                if (this.mouse.burning > 0)
                {
                    this.light.setAlpha = Mathf.Lerp(0.5f, 1f, Random.value) * 1f - 0.6f * this.mouse.LightIntensity;
                    this.light.setRad = Mathf.Max(this.mouse.flashRad, Mathf.Lerp(60f, 290f, Random.value) * 1f + this.mouse.LightIntensity * 10f) * (this.mouse.charge / 10);
                } 
                else
                {
                    this.light.setAlpha = 0.5f * (1f - 0.6f * this.mouse.LightIntensity);
                    this.light.setRad = Mathf.Max(this.mouse.flashRad, 80f * 1f + this.mouse.LightIntensity * 10f) * (this.mouse.charge / 10);
                }
                if (this.light.slatedForDeletetion || this.light.room != this.mouse.room)
                {
                    this.light = null;
                    return;
                }
            } else if (this.mouse.room.Darkness(this.bulbPos) > 0f)
            {
                this.light = new LightSource(this.tail[tail.Length - 1].pos, false, Color.HSVToRGB(this.mouse.iVars.color.hue - 0.05f, this.mouse.iVars.color.saturation, this.mouse.iVars.color.lightness + 0.1f), this.mouse);
                this.mouse.room.AddObject(light);
            }
        }

        public override void Reset(Vector2 resetPoint)
        {
            base.Reset(resetPoint);
            for(int i = 0; i < tail.Length;i++)
            {
                this.tail[i].Reset(resetPoint);
            }
        }

        public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            Vector2 vector = Vector2.Lerp(mouse.bodyChunks[1].lastPos, mouse.bodyChunks[1].pos, timeStacker);
            Vector2 vector2 = Vector2.Lerp(mouse.bodyChunks[0].lastPos, mouse.bodyChunks[0].pos, timeStacker);
            Vector2 vector3 = Vector2.Lerp(vector, vector2, timeStacker);
            float d2 = 2f;
            for (int i = 0; i < tail.Length; i++)
            {
                (sLeaser.sprites[graphics.TailSprite] as TriangleMesh).color = graphics.BodyColor;
                Vector2 vector4 = Vector2.Lerp(this.tail[i].lastPos, this.tail[i].pos, timeStacker);
                Vector2 normalized = (vector4 - vector3).normalized;
                Vector2 a = Custom.PerpendicularVector(normalized);
                float d3 = Vector2.Distance(vector4, vector3) / 5f;
                if (i == 0)
                {
                    d3 = 0f;
                }
                (sLeaser.sprites[graphics.TailSprite] as TriangleMesh).MoveVertice(i * 4, vector3 - a * d2 + normalized * d3 - camPos);
                (sLeaser.sprites[graphics.TailSprite] as TriangleMesh).MoveVertice(i * 4 + 1, vector3 + a * d2 + normalized * d3 - camPos);
                if (i < tail.Length - 1)
                {
                    (sLeaser.sprites[graphics.TailSprite] as TriangleMesh).MoveVertice(i * 4 + 2, vector4 - a * this.tail[i].StretchedRad - normalized * d3 - camPos);
                    (sLeaser.sprites[graphics.TailSprite] as TriangleMesh).MoveVertice(i * 4 + 3, vector4 + a * this.tail[i].StretchedRad - normalized * d3 - camPos);
                }
                else
                {
                    (sLeaser.sprites[graphics.TailSprite] as TriangleMesh).MoveVertice(i * 4 + 2, vector4 - camPos);
                }
                
                d2 = this.tail[i].StretchedRad;
                vector3 = vector4;
            }

            Vector2 bulbVec = Vector2.Lerp(this.tail[this.tail.Length - 1].lastPos, this.tail[this.tail.Length - 1].pos, timeStacker);
            Vector2 bulbRot = Vector2.Lerp(this.tail[this.tail.Length - 2].lastPos, this.tail[this.tail.Length - 2].pos, timeStacker);

            if (this.mouse.burning == 0)
            {
                sLeaser.sprites[graphics.FlareSprite].shader = rCam.room.game.rainWorld.Shaders["FlatLightBehindTerrain"];
                sLeaser.sprites[graphics.FlareSprite].x = bulbVec.x - camPos.x;
                sLeaser.sprites[graphics.FlareSprite].y = bulbVec.y - camPos.y;
                sLeaser.sprites[graphics.FlareSprite].scale = 2.5f;
            } else
            {
                sLeaser.sprites[graphics.FlareSprite].shader = rCam.room.game.rainWorld.Shaders["FlareBomb"];
                sLeaser.sprites[graphics.FlareSprite].x = vector.x - camPos.x + Mathf.Lerp(this.mouse.lastFlickerDir.x, this.mouse.flickerDir.x, timeStacker);
                sLeaser.sprites[graphics.FlareSprite].y = vector.y - camPos.y + Mathf.Lerp(this.mouse.lastFlickerDir.y, this.mouse.flickerDir.y, timeStacker);
                sLeaser.sprites[graphics.FlareSprite].scale = Mathf.Lerp(this.mouse.lastFlashRad, this.mouse.flashRad, timeStacker) / 16f;
                sLeaser.sprites[graphics.FlareSprite].alpha = Mathf.Lerp(this.mouse.lastFlashAlpha, this.mouse.flashAplha, timeStacker);
            }

            sLeaser.sprites[graphics.BulbSprite].x = bulbVec.x - camPos.x;
            sLeaser.sprites[graphics.BulbSprite].y = bulbVec.y - camPos.y;
            sLeaser.sprites[graphics.BulbSprite].rotation = Custom.AimFromOneVectorToAnother(bulbVec, bulbRot);
            sLeaser.sprites[graphics.BulbSprite].color = Color.Lerp(Color.black, Color.white, (this.mouse.charge / 8f));
        }
    }
}
