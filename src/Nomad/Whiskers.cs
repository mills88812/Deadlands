﻿using UnityEngine.Assertions;

namespace Deadlands;

/// <summary>
/// Stores the graphics data for the Whiskers of a Nomad slugcat.
/// </summary>
/// <param name="owner">The main body of the slugcat, used for determining head/body/tail positioning.</param>
/// <param name="startSprite">The index into the <see cref="RoomCamera.SpriteLeaser"/>'s array where the Whiskers store their sprites.</param>
internal sealed class Whiskers(PlayerGraphics owner, int startSprite)
{
    private readonly PlayerGraphics _pGraphics = owner;

    private readonly int _startSprite = startSprite;

    /// <summary> How many whiskers to give them.</summary>
    private const int _whiskerCount = 4;

    public const int RequiredSprites = _whiskerCount;

    public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        Assert.IsTrue(_whiskerCount % 2 == 0); // Has to be an even number of whiskers.
        for (int i = 0; i < RequiredSprites; i++)
        {
            sLeaser.sprites[_startSprite + i] = Utils.CreateSimpleMesh();
            sLeaser.sprites[_startSprite + i].color = Color.white;
        }
    }

    public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        var playerHead = Vector2.Lerp(_pGraphics.head.lastPos,_pGraphics.head.pos,timeStacker);
        for (int i = 0; i < RequiredSprites; i++)
        {
            var mesh = sLeaser.sprites[_startSprite + i] as TriangleMesh;
            Assert.IsNotNull(mesh);

            // Various moustache properties (all measured in pixels)
            const int offset = 3; // how far we are from the nose
            const int lengthBend = 5; // the horizontal girth of the bent part
            const int lengthStraight = 3; // the girth of the straightaway part
            const int height = 1; // the height
            int verticalOrigin = i * -2 * height; // How up we are from the nose (differs between whiskers)

            Vector2[] moustacheStash = [
                    new Vector2(-lengthStraight - offset - lengthBend, verticalOrigin),
                    new Vector2(-lengthStraight - offset - lengthBend*0.9f, verticalOrigin - height*0.9f),
                    new Vector2(-lengthStraight - offset, height + verticalOrigin),
                    new Vector2(-lengthStraight - offset, verticalOrigin),
                    new Vector2(-offset, height + verticalOrigin),
                    new Vector2(-offset, verticalOrigin)
            ];
            if (i % 2 == 0) // if it should go on the right side
                for (int m = 0; m < 6; m++)
                    moustacheStash[m].x *= -1.0f; // flip the vertices :^)

            var headVertexLoc = playerHead - camPos;
            for(int v = 0; v < 6; v++)
                mesh.MoveVertice(v, headVertexLoc + moustacheStash[v]);
        }
    }

    public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        // Do nothing, we're always white :^)
    }

    public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContainer)
    {
        newContainer ??= rCam.ReturnFContainer("Midground");
        for (int i = 0; i < RequiredSprites; i++)
        {
            newContainer.AddChild(sLeaser.sprites[_startSprite + i]);
        }
    }
}