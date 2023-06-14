using RWCustom;
using UnityEngine;

namespace Deadlands;

sealed class Wing : PhysicalObject, IDrawable
{
    public Player player;
    public int index;

    public Color wingColor;


    public WingAbstract Abstr { get; }

    public Wing(WingAbstract abstr, Vector2 pos, Vector2 vel) : base(abstr)
    {
        Abstr = abstr;

        base.bodyChunks = new[] { new BodyChunk(this, 0, pos + vel, 0, 0.35f) { goThroughFloors = true } };
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
        //sLeaser.sprites[0] = new FSprite("CentipedeBackShell", true);
        //sLeaser.sprites[1] = new FSprite("CentipedeBackShell", true);

        TriangleMesh.Triangle[] tris = new TriangleMesh.Triangle[]
        {
            new TriangleMesh.Triangle(0, 1, 2),
            new TriangleMesh.Triangle(3, 4, 5),
            new TriangleMesh.Triangle(6, 7, 8)
        };

        TriangleMesh triangleMesh = new TriangleMesh("Futile_White", tris, false, false);
        sLeaser.sprites[0] = triangleMesh;

        AddToContainer(sLeaser, rCam, null);
    }


    public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        Vector2 shoulder = Vector2.Lerp(player.bodyChunks[0].lastPos, player.bodyChunks[0].pos, timeStacker);
        Vector2 hips = Vector2.Lerp(player.bodyChunks[1].lastPos, player.bodyChunks[1].pos, timeStacker);

        Vector2 hand = Vector2.Lerp(player.BodyPartByIndex(index).lastPos, player.BodyPartByIndex(index).pos, timeStacker)
            + 1.5f * (shoulder - hips).normalized;

        // (Not a vertice)
        var hand_shoulder_dist = Mathf.Min(Vector2.Distance(hand, shoulder), 1);

        // Inbetween vertices

        Vector2 hand_to_shoulder =
            Vector2.Lerp(hand, shoulder, 0.5f);
        
        shoulder += 3 * Vector2.up;

        
        var hand_hips_diff = index == 0 ? hips - hand : hand - hips;

        Vector2 hand_to_hips = hand_shoulder_dist *
            0.1f * new Vector2(hand_hips_diff.y, -hand_hips_diff.x) +
            Vector2.Lerp(hips, hand, 0.5f);

        // Visualize this as triangles being created from the vertices from left to right (on the left wing)
        // All triangles are realized clock-wise (doesn't matter since it's a 2D game, but I did it anyways)

        ((TriangleMesh)sLeaser.sprites[0]).MoveVertice(0, hand - camPos);
        ((TriangleMesh)sLeaser.sprites[0]).MoveVertice(1, hand_to_shoulder - camPos);   // Triangle 1
        ((TriangleMesh)sLeaser.sprites[0]).MoveVertice(2, hand_to_hips - camPos);

        ((TriangleMesh)sLeaser.sprites[0]).MoveVertice(3, hand_to_hips - camPos);
        ((TriangleMesh)sLeaser.sprites[0]).MoveVertice(4, hand_to_shoulder - camPos);   // Triangle 2
        ((TriangleMesh)sLeaser.sprites[0]).MoveVertice(5, hips - camPos);

        ((TriangleMesh)sLeaser.sprites[0]).MoveVertice(6, hand_to_shoulder - camPos);
        ((TriangleMesh)sLeaser.sprites[0]).MoveVertice(7, shoulder - camPos);           // Triangle 3
        ((TriangleMesh)sLeaser.sprites[0]).MoveVertice(8, hips - camPos);


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