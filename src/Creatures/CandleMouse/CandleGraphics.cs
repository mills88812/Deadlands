using Deadlands;
using RWCustom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Deadlands.Creatures.CandleMouse;

internal class CandleGraphics : GraphicsModule, ILookingAtCreatures
{
    public CandleMouse mouse { get { return owner as CandleMouse; } }

    public int TotalSprites { get { return 14; } }
    public int EyeASprite(int side)
    {
        return 8 + side;
    }
    public int EyeBSprite(int side)
    {
        return 10 + side;
    }
    public int EyeCSprite(int side)
    {
        return 12 + side;
    }
    public int FlareSprite { get { return 7; } }
    public int BulbSprite { get { return 6; } }
    public int TailSprite { get { return 5; } }

    public int HeadSprite { get {return 4; } }

    public Color BodyColor {
        get
        {
            HSLColor from = new HSLColor(this.mouse.iVars.color.hue, 1f * 0.3f, 0.9f * 0.5f);
            return Color.Lerp(from.rgb, this.blackColor, this.litRoom * 0.6f);
        }
    }

    public Color DecalColor
    {
        get
        {
            HSLColor from = new HSLColor(this.mouse.iVars.color.hue, 0.3f, 0.45f);
            return Color.Lerp(from.rgb, this.blackColor, this.litRoom * 0.6f);
        }
    }

    public Color EyeColor
    {
        get
        {
            if (this.mouse.burning == 0f)
            {
                return Color.white;
            }
            return Color.Lerp(Color.white, this.blackColor, Mathf.InverseLerp(0f, 0.25f, this.mouse.burning));
        }
    }

    private int BodySprite(int bodyChunk) { return bodyChunk; }

    private int LimbSprite(int limb) { return 2 + limb; }

    public CreatureLooker creatureLooker;

    public BodyPart head;
    public FlashTail tail;

    private Limb[] limbs;
    public Vector2 lookDir;
    public Vector2 lastLookDir;
    public float backToCam;
    private float lastVibrate;
    private float vibrate;
    private float lastProfileFac;
    private float litRoom;
    private float lastRunCycle;
    private float runCycle;
    private float lastBackToCam;
    private float profileFac;
    private Color blackColor;
    private int ouchEyes;
    private int blink;

    public CandleGraphics(PhysicalObject ow) : base(ow, false)
    {
        this.creatureLooker = new CreatureLooker(this, this.mouse.AI.tracker, this.mouse, 0.2f, 50);
        this.head = new BodyPart(this);
        this.tail = new FlashTail(this);
        this.bodyParts = new BodyPart[4];
        this.limbs = new Limb[2];
        for (int i = 0; i < 2; i++)
        {
            this.limbs[i] = new Limb(this, this.mouse.bodyChunks[1], i, 1f, 0.5f, 0.9f, 8f, 0.9f);
            this.bodyParts[i] = this.limbs[i];
        }
        this.bodyParts[2] = this.head;
        this.bodyParts[3] = this.tail;

        this.DEBUGLABELS = new DebugLabel[2];
        this.DEBUGLABELS[0] = new DebugLabel(ow, new Vector2(40f, 50f));
        this.DEBUGLABELS[1] = new DebugLabel(ow, new Vector2(40f, 40f));

        this.Reset();
    }

    public override void Update()
    {
        base.Update();
        this.lastVibrate = this.vibrate;
        this.vibrate = Custom.LerpAndTick(this.vibrate, (this.mouse.jumpCharging > 0f) ? 1f : 0f, 0.2f, 0.05f);
        this.lastProfileFac = this.profileFac;
        this.litRoom = Mathf.Pow(1f - Mathf.InverseLerp(0f, 0.5f, this.mouse.room.Darkness(this.mouse.mainBodyChunk.pos)), 3f);

        this.lastRunCycle = this.runCycle;
        this.runCycle = this.mouse.runCycle;
        this.lastBackToCam = this.backToCam;
        this.lastLookDir = this.lookDir;

        this.head.Update();
        this.head.lastPos = this.head.pos;
        this.head.pos += this.head.vel;
        Vector2 vector = this.mouse.mainBodyChunk.pos + Custom.DirVec(this.mouse.bodyChunks[1].pos, this.mouse.mainBodyChunk.pos) * (3f + 3f * Mathf.Abs(this.profileFac));
        this.head.ConnectToPoint(vector, 4f, false, 0f, this.mouse.mainBodyChunk.vel, 0.5f, 0.1f);
        this.head.vel += (vector - this.head.pos) / 6f;
        this.head.vel += this.lookDir;
        if (!this.mouse.Consious)
        {
            BodyPart bodyPart = this.head;
            bodyPart.vel.y = bodyPart.vel.y - 0.6f;
        }
        this.tail.Update();
        this.tail.lastPos = this.tail.pos;
        this.tail.pos += this.tail.vel;
        this.tail.pos += Custom.DegToVec(Random.value * 360f) * 2f * Random.value;
        this.tail.ConnectToPoint(vector, 7f, false, 0f, this.mouse.bodyChunks[1].vel, 0.1f, 0f);
        this.tail.TailUpdate();

        this.blink--;
        if ((this.blink < 0) && Random.value < 0.2f || this.blink < -15)
        {
            this.blink = Random.Range(3, 4);
        }
        if (this.mouse.voiceCounter > 0)
        {
            this.blink = Mathf.Max(this.blink, 1);
        }

        Vector2 vector2 = Custom.DirVec(this.mouse.bodyChunks[1].pos, this.mouse.bodyChunks[0].pos);
        Vector2 a2 = Custom.PerpendicularVector(vector2) * Mathf.Lerp(1f, -1f, this.backToCam);

        for (int i = 0; i < 2; i++)
        {
            this.limbs[i].Update();
            this.limbs[i].ConnectToPoint(this.mouse.bodyChunks[1].pos, 12f, false, 0f, this.mouse.bodyChunks[1].vel, 0f, 0f);
            Limb limb = this.limbs[i];
            limb.vel.y = limb.vel.y - 0.6f;
            if (!this.limbs[i].retract)
            {
                this.limbs[i].mode = Limb.Mode.Dangle;
            }
            if (this.mouse.Consious)
            {
                float num = Mathf.Sin((this.runCycle + (float)this.limbs[i].limbNumber / 4f) * 3.1415927f * 2f);
                Vector2 goalPos = this.mouse.mainBodyChunk.pos + vector2 * 8f * (0.3f + 0.7f * num) + a2 * 4f * ((i == 0) ? -1f : 1f);
                this.limbs[i].FindGrip(this.mouse.room, this.mouse.mainBodyChunk.pos, this.mouse.mainBodyChunk.pos, 15f, goalPos, 2, 2, false);
                this.limbs[i].pos += vector2 * (2f + num * 8f) * Mathf.Lerp(0.5f, 2f, this.mouse.AI.fear);
                this.limbs[i].pos -= a2 * ((i == 0) ? -1f : 1f) * 3f * Mathf.Cos((this.runCycle + (float)this.limbs[i].limbNumber / 4f) * 3.1415927f * 2f) * Mathf.Lerp(0.5f, 2f, this.mouse.AI.fear);
            }
        }
    }

    public override void Reset()
    {
        base.Reset();
        foreach (BodyPart bodyPart in this.bodyParts)
        {
            bodyPart.vel *= 0f;
            bodyPart.pos = this.mouse.mainBodyChunk.pos;
            bodyPart.lastPos = bodyPart.pos;
        }
        tail.Reset(this.mouse.mainBodyChunk.pos);
    }

    public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        this.blackColor = palette.blackColor;
        base.ApplyPalette(sLeaser, rCam, palette);
    }

    public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
    {
        sLeaser.RemoveAllSpritesFromContainer();
        if (newContatiner == null)
        {
            newContatiner = rCam.ReturnFContainer("Midground");
        }
        for (int i = 0; i < this.TotalSprites; i++)
        {
            if (sLeaser.sprites[i] != null)
            {
                //Debug.Log(sLeaser.sprites[i]);
                if (i == FlareSprite)
                {
                    rCam.ReturnFContainer("Water").AddChild(sLeaser.sprites[i]);
                }
                else
                {
                    newContatiner.AddChild(sLeaser.sprites[i]);
                }
            } else
            {
                Debug.Log("NULL!");
            }
        }
    }

    // 0 - 1 BodyChunk | 2 - 3 Limb | 4 Head | 5 TailSegments | 6 Bulb | 7 Flare | 8 - 9 EyeA | 10 - 11 EyeB | 12 - 13 EyeC
    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        sLeaser.sprites = new FSprite[this.TotalSprites];

        for (int i = 0; i < this.TotalSprites; i++)
        {
            if (i == 5)
            {
                sLeaser.sprites[i] = TriangleMesh.MakeLongMesh(this.tail.tail.Length, true, true); // Tail Mesh
            }
            else
            {
                sLeaser.sprites[i] = new FSprite("pixel", true);
            }
        }
        sLeaser.sprites[this.EyeASprite(1)].scaleX = -1;
        sLeaser.sprites[this.EyeBSprite(1)].scaleX = -1;
        for (int i = 0; i < 2; i++)
        {
            sLeaser.sprites[this.LimbSprite(i)].element = Futile.atlasManager.GetElementWithName("mouseHindLeg");
            sLeaser.sprites[this.LimbSprite(i)].anchorY = 0.1f;
        }
        sLeaser.sprites[this.BodySprite(0)].element = Futile.atlasManager.GetElementWithName("mouseBodyA");
        sLeaser.sprites[this.BodySprite(1)].element = Futile.atlasManager.GetElementWithName("mouseBodyB");
        sLeaser.sprites[this.BodySprite(1)].anchorY = 0f;
        sLeaser.sprites[this.HeadSprite].element = Futile.atlasManager.GetElementWithName("mouseHead0");
        sLeaser.sprites[this.BulbSprite].element = Futile.atlasManager.GetElementWithName("Pebble5");

        sLeaser.sprites[this.FlareSprite].element = Futile.atlasManager.GetElementWithName("Futile_White");
        sLeaser.sprites[this.FlareSprite].scale = 2.5f;
        this.AddToContainer(sLeaser, rCam, null);
        base.InitiateSprites(sLeaser, rCam);
    }

    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {

        // Colors

        sLeaser.sprites[this.HeadSprite].color = this.BodyColor;
        for (int i = 0; i < 2;i++)
        {
            sLeaser.sprites[this.BodySprite(i)].color = this.BodyColor;
            sLeaser.sprites[this.LimbSprite(i)].color = this.BodyColor;

            sLeaser.sprites[this.EyeASprite(i)].color = this.DecalColor;
            sLeaser.sprites[this.EyeBSprite(i)].color = this.EyeColor;
        }

        // Vectors

        Vector2 vector = Vector2.Lerp(this.mouse.bodyChunks[1].lastPos, this.mouse.bodyChunks[1].pos, timeStacker);
        Vector2 vector2 = Vector2.Lerp(this.mouse.bodyChunks[0].lastPos, this.mouse.bodyChunks[0].pos, timeStacker);
        float rCycle = Mathf.Lerp(this.lastRunCycle, this.runCycle, timeStacker);
        Vector2 vector3 = Vector2.Lerp(this.tail.lastPos, this.tail.pos, timeStacker);
        Vector2 vector4 = Vector2.Lerp(this.head.lastPos, this.head.pos, timeStacker);
        if (this.mouse.Consious && this.mouse.voiceCounter > 0)
        {
            vector4 += Custom.RNV() * Random.value * 2f;
        }
        float profile = Mathf.Lerp(this.lastProfileFac, this.profileFac, timeStacker);
        float backCam = Mathf.Lerp(this.lastBackToCam, this.backToCam, timeStacker);
        Vector2 vector5 = Custom.DirVec(vector, vector2);
        Vector2 a = Custom.PerpendicularVector(vector5);

        float rotation = Custom.AimFromOneVectorToAnother(vector, vector2);

        // Limbs

        for (int i = 0; i < 2; i++)
        {
            Vector2 vector6 = Vector2.Lerp(this.limbs[i].lastPos, this.limbs[i].pos, timeStacker);
            Vector2 vector7 = vector2;
            vector7 += a * ((i == 0) ? -1f : 1f) * 3f * (1f - Mathf.Abs(profile));
            if (i == 1)
            {
                vector7 -= vector5 * 2f;
            }
            if (!Custom.DistLess(vector6, vector7, 19f))
            {
                vector6 = vector7 + Custom.DirVec(vector7, vector6) * 19f;
            }
            sLeaser.sprites[this.LimbSprite(i)].x = vector6.x - camPos.x;
            sLeaser.sprites[this.LimbSprite(i)].y = vector6.y - camPos.y;
            sLeaser.sprites[this.LimbSprite(i)].rotation = Custom.AimFromOneVectorToAnother(vector6, vector7);
            sLeaser.sprites[this.LimbSprite(i)].scaleY = Mathf.Lerp(Vector2.Distance(vector6, vector7) / 18f, 1f, 0.2f);
            sLeaser.sprites[this.LimbSprite(i)].scaleX = Mathf.Sign(Custom.DistanceToLine(vector6, vector2 - a * ((i == 0) ? -1f : 1f), vector - a * ((i == 0) ? -1f : 1f)));
            sLeaser.sprites[this.LimbSprite(i)].isVisible = (this.limbs[i].mode != Limb.Mode.Retracted);
        }

        // Body

        sLeaser.sprites[this.BodySprite(1)].x = Mathf.Lerp(vector.x, vector2.x, 0.5f) - camPos.x;
        sLeaser.sprites[this.BodySprite(1)].y = Mathf.Lerp(vector.y, vector2.y, 0.5f) - camPos.y;
        sLeaser.sprites[this.BodySprite(1)].rotation = Custom.AimFromOneVectorToAnother(Vector2.Lerp(vector, vector2, 0.34f), vector3);
        sLeaser.sprites[this.BodySprite(1)].scaleY = (Vector2.Distance(Vector2.Lerp(vector, vector2, 0.34f), vector3) + 5f) / 24f;
        sLeaser.sprites[this.BodySprite(1)].scaleX = 1f;
        sLeaser.sprites[this.BodySprite(0)].x = Mathf.Lerp(vector.x, vector2.x, 0.75f) - camPos.x;
        sLeaser.sprites[this.BodySprite(0)].y = Mathf.Lerp(vector.y, vector2.y, 0.75f) - camPos.y;
        sLeaser.sprites[this.BodySprite(0)].rotation = Custom.AimFromOneVectorToAnother(Vector2.Lerp(vector2, vector4, 0.3f), vector);

        // Head

        float facing = Custom.AimFromOneVectorToAnother(Vector2.Lerp(vector, vector2, 0.5f), vector4) - 180f * backCam;
        facing -= 90f * profile;
        sLeaser.sprites[this.HeadSprite].x = vector4.x - camPos.x;
        sLeaser.sprites[this.HeadSprite].y = vector4.y - camPos.y;
        Vector2 vector12 = Vector2.Lerp(this.lastLookDir, this.lookDir, timeStacker);
        Vector2 vector13 = Custom.RotateAroundOrigo(vector12, -facing);
        sLeaser.sprites[this.BodySprite(0)].scaleY = Mathf.Lerp(Mathf.Lerp(1.2f, 0.8f, backCam), Mathf.Lerp(0.8f, 1.2f, backCam), Mathf.InverseLerp(1f, -1f, vector13.y));
        int num11 = Mathf.FloorToInt(Mathf.Abs(profile) * 4.9f);
        if (num11 > 0)
        {
            sLeaser.sprites[this.HeadSprite].element = Futile.atlasManager.GetElementWithName("mouseHead" + num11.ToString());
        }
        else if (vector13.y > 0.75f)
        {
            sLeaser.sprites[this.HeadSprite].element = Futile.atlasManager.GetElementWithName("mouseHeadUp");
        }
        else if (vector13.y < -0.75f)
        {
            sLeaser.sprites[this.HeadSprite].element = Futile.atlasManager.GetElementWithName("mouseHeadDown");
        }
        else
        {
            sLeaser.sprites[this.HeadSprite].element = Futile.atlasManager.GetElementWithName("mouseHead0");
        }
        sLeaser.sprites[this.HeadSprite].scaleX = (profile > 0f) ? 1f : -1f;
        sLeaser.sprites[this.HeadSprite].rotation = facing;

        for (int i = 0; i < 2;  i++)
        {
            bool flag = true;
            bool white = true;
            float eyeFacing = facing;
            Vector2 vec = new Vector2(4f, -2f);
            if (i == 0 == vector13.x < 0f)
            {
                sLeaser.sprites[this.EyeASprite(i)].scaleX = Mathf.Lerp(1f, 0f, Mathf.InverseLerp(0.1f, 0.4f, Mathf.Abs(profile))) * ((i == 1) ? -1f : 1f);
                sLeaser.sprites[this.EyeBSprite(i)].scaleX = Mathf.Lerp(1f, 0f, Mathf.InverseLerp(0.1f, 0.4f, Mathf.Abs(profile))) * ((i == 1) ? -1f : 1f);
                vec.x += 5f * Mathf.Abs(profile);
                vec.y += 2.5f * Mathf.Abs(profile);
                eyeFacing += Mathf.Sign(vector13.x) * Mathf.InverseLerp(0.1f, 0.4f, Mathf.Abs(profile)) * -25f;
                flag = (Mathf.Abs(profile) < 0.1f);
                white = (Mathf.Abs(profile) < 0.2f);
            }
            else
            {
                if (Mathf.Abs(profile) > 0.1f)
                {
                    sLeaser.sprites[this.EyeASprite(i)].scaleX = Mathf.Lerp(1f, 0.85714f, Mathf.Abs(profile)) * ((i == 1) ? -1f : 1f);
                    sLeaser.sprites[this.EyeBSprite(i)].scaleX = Mathf.Lerp(1f, 0.85714f, Mathf.Abs(profile)) * ((i == 1) ? -1f : 1f);
                    flag = false;
                }
                vec.x -= 4.5f * Mathf.Pow(Mathf.Abs(profile), 0.6f);
            }
            vec.x *= ((i == 0) ? -1f : 1f);
            if (Mathf.Abs(vector13.y) > 0.75f)
            {
                sLeaser.sprites[this.EyeASprite(i)].scaleY = 0.85714f;
                sLeaser.sprites[this.EyeBSprite(i)].scaleY = 0.85714f;
                flag = false;
                vec.y += vector13.y * ((vector13.y > 0f) ? 2f : 1f);
            }
            else
            {
                sLeaser.sprites[this.EyeASprite(i)].scaleY = 1f;
                sLeaser.sprites[this.EyeBSprite(i)].scaleY = 1f;
            }
            Vector2 eyeVector = vector4 + vector12;
            eyeVector += Custom.RotateAroundOrigo(vec, facing);
            int eyeIndex;
            if (this.mouse.Consious)
            {
                if (this.ouchEyes > 0)
                {
                    eyeIndex = 2;
                } 
                else
                {
                    eyeIndex = (this.blink > 0) ? 1 : 3;
                }
            } else if (this.mouse.dead)
            {
                eyeIndex = 5;
            } else
            {
                eyeIndex = 2;
            }
            sLeaser.sprites[this.EyeASprite(i)].x = eyeVector.x - camPos.x;
            sLeaser.sprites[this.EyeASprite(i)].y = eyeVector.y - camPos.y;
            sLeaser.sprites[this.EyeBSprite(i)].x = eyeVector.x - camPos.x;
            sLeaser.sprites[this.EyeBSprite(i)].y = eyeVector.y - camPos.y;
            if (white && eyeIndex == 1)
            {
                sLeaser.sprites[this.EyeCSprite(i)].isVisible = true;
                sLeaser.sprites[this.EyeCSprite(i)].x = eyeVector.x - camPos.x - 1f;
                sLeaser.sprites[this.EyeCSprite(i)].y = eyeVector.y - camPos.y + 1f;
            }
            else
            {
                sLeaser.sprites[this.EyeCSprite(i)].isVisible = false;
            }
            if (flag && eyeIndex == 1)
            {
                sLeaser.sprites[this.EyeASprite(i)].rotation = 0f;
                sLeaser.sprites[this.EyeBSprite(i)].rotation = 0f;
            }
            else
            {
                sLeaser.sprites[this.EyeASprite(i)].rotation = eyeFacing;
                sLeaser.sprites[this.EyeBSprite(i)].rotation = eyeFacing;
            }
            sLeaser.sprites[this.EyeASprite(i)].element = Futile.atlasManager.GetElementWithName("mouseEyeA" + eyeIndex.ToString());
            sLeaser.sprites[this.EyeBSprite(i)].element = Futile.atlasManager.GetElementWithName("mouseEyeB" + eyeIndex.ToString());
        }

        this.tail.DrawSprites(sLeaser, rCam, timeStacker, camPos);
        base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
    }

    public float CreatureInterestBonus(Tracker.CreatureRepresentation crit, float score)
    {
        if (crit.representedCreature.creatureTemplate.type == DLCreature.CandleMouse)
        {
            score *= 0.04f;
        }
        return score;
    }

    public Tracker.CreatureRepresentation ForcedLookCreature()
    {
        return null;
    }

    public void LookAtNothing()
    {
    }
}
