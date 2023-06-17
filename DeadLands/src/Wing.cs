using RWCustom;
using UnityEngine;

namespace Deadlands;

sealed class Wing : PhysicalObject, IDrawable
{
    public Player player;
    public int index;

    public Color wingColor;

    public float size = 0.12f;
    public float pointiness = 0.7f; // Ranges from 0 - 1, one being max, zero being none at all


    public WingAbstract Abstr { get; }

    public Wing(WingAbstract abstr, Vector2 pos, Vector2 vel) : base(abstr)
    {
        Abstr = abstr;

        base.bodyChunks = new[] { new BodyChunk(this, 0, pos + vel, 0, 1) { goThroughFloors = true } };
        base.bodyChunks[0].lastPos = base.bodyChunks[0].pos;
        base.bodyChunks[0].vel = vel;

        base.bodyChunkConnections = new BodyChunkConnection[0];
        base.airFriction = 1f;
        base.gravity = 0f;
        base.bounce = 0f;
        base.surfaceFriction = 0.45f;
        base.collisionLayer = 1;
        base.waterFriction = 0.92f;
        base.buoyancy = 0f;

        base.CollideWithObjects = false;
        base.CollideWithSlopes = false;
        base.CollideWithTerrain = false;
    }

    public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        sLeaser.sprites = new FSprite[1];

        // Visualize this as triangles being created from the vertices from left to right (on the left wing)
        // All triangles are realized clock-wise (Probably doesn't matter cause it's a 2D game, but I did it anyways)
        TriangleMesh.Triangle[] tris = new TriangleMesh.Triangle[]
        {
            new TriangleMesh.Triangle(0, 2, 1),
            new TriangleMesh.Triangle(1, 2, 3),
            new TriangleMesh.Triangle(2, 4, 3),
            new TriangleMesh.Triangle(3, 4, 5)
        };

        TriangleMesh triangleMesh = new TriangleMesh("Futile_White", tris, false, false);
        sLeaser.sprites[0] = triangleMesh;

        AddToContainer(sLeaser, rCam, null);
    }


    public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        /////////////////////////////////////
        // Base points
        /////////////////////////////////////

        Vector2 shoulder = Vector2.Lerp(player.bodyChunks[0].lastPos, player.bodyChunks[0].pos, timeStacker);
        Vector2 hip = Vector2.Lerp(player.bodyChunks[1].lastPos, player.bodyChunks[1].pos, timeStacker);

        Vector2 hand = Vector2.Lerp(player.BodyPartByIndex(index).lastPos, player.BodyPartByIndex(index).pos, timeStacker)
            + 1.5f * (shoulder - hip).normalized;

        // (All Vector2s initalized with "var" are not vertices)
        var hand_shoulder_dist = 0.1f * Vector2.Distance(hand, shoulder);

        /////////////////////////////////////
        // Inbetween points
        /////////////////////////////////////

        var hand_shoulder_diff = index == 0 ? hand - shoulder : shoulder - hand;

        // Potential division by zero so we have to do silly branching stuff :spearboowomp:
        Vector2 hand_shoulder_inbetween = (
            Vector2.Lerp(shoulder, hand, 0.5f)
        );
        

        shoulder += 3 * Vector2.up;

        
        var hand_hips_diff = index == 0 ? hip - hand : hand - hip;

        var hand_hips_inbetween = (

            size * hand_shoulder_dist * new Vector2(hand_hips_diff.y, -hand_hips_diff.x) +

            Vector2.Lerp(hip, hand, 0.5f)
        );

        Vector2 hand_to_inbetween = Vector2.Lerp(hand, hand_hips_inbetween, pointiness);
        Vector2 hips_to_inbetween = Vector2.Lerp(hip, hand_hips_inbetween, pointiness);

        /////////////////////////////////////
        // Vertex assignment
        /////////////////////////////////////

        ((TriangleMesh)sLeaser.sprites[0]).MoveVertice(0, hand - camPos);
        ((TriangleMesh)sLeaser.sprites[0]).MoveVertice(1, hand_to_inbetween - camPos);
        ((TriangleMesh)sLeaser.sprites[0]).MoveVertice(2, hand_shoulder_inbetween - camPos);

        ((TriangleMesh)sLeaser.sprites[0]).MoveVertice(3, hips_to_inbetween - camPos);
        ((TriangleMesh)sLeaser.sprites[0]).MoveVertice(4, shoulder - camPos);
        ((TriangleMesh)sLeaser.sprites[0]).MoveVertice(5, hip - camPos);


        sLeaser.sprites[0].color = wingColor;


        if (slatedForDeletetion || room != rCam.room) {
            sLeaser.CleanSpritesAndRemove();
        }
    }

    public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        wingColor = Color.white;
    }

    public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer? newContainer)
    {
        newContainer ??= rCam.ReturnFContainer("Items");

        foreach (FSprite fsprite in sLeaser.sprites) {
            fsprite.RemoveFromContainer();
            newContainer.AddChild(fsprite);

            newContainer.MoveToBack();
        }
    }
}