using RWCustom;
using System;
using System.Reflection;
using UnityEngine;

namespace Deadlands;

sealed class Wings
{
    private PlayerGraphics pGraphics;

    private int startSprite;

    public float size = 0.12f;
    public float pointiness = 0.7f; // Ranges from 0 - 1, one being max, zero being none at all



    public Wings(PlayerGraphics owner, int startSprite, float size = 0.12f, float pointiness = 0.7f)
    {
        this.pGraphics = owner;
        this.startSprite = startSprite;

        this.size = size;
        this.pointiness = pointiness;
    }

    public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        for (int i = 0; i < 2; i++)
        {
            // Visualize this as triangles being created from the vertices from left to right (on the left wing)
            // All triangles are realized clock-wise (Probably doesn't matter cause it's a 2D game, but I did it anyways)
            TriangleMesh.Triangle[] tris = new TriangleMesh.Triangle[]
            {
                new TriangleMesh.Triangle(0, 2, 1),
                new TriangleMesh.Triangle(1, 2, 3),
                new TriangleMesh.Triangle(2, 4, 3),
                new TriangleMesh.Triangle(3, 4, 5)
            };

            //Debug.Log(startSprite + i);
            sLeaser.sprites[startSprite + i] = new TriangleMesh("Futile_White", tris, false, false);
        }
    }


    public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        for (int i = 0; i < 2; i++)
        {
            PlaceWingVerts(sLeaser, timeStacker, camPos, startSprite + i, i);
        }
    }

    private void PlaceWingVerts(RoomCamera.SpriteLeaser sLeaser, float timeStacker, Vector2 camPos, int startSprite, int index)
    {
        /////////////////////////////////////
        // Base points
        /////////////////////////////////////

        Vector2 shoulder = Vector2.Lerp(pGraphics.owner.bodyChunks[0].lastPos, pGraphics.owner.bodyChunks[0].pos, timeStacker);
        Vector2 hip = Vector2.Lerp(pGraphics.owner.bodyChunks[1].lastPos, pGraphics.owner.bodyChunks[1].pos, timeStacker);

        var shoulder_hip_diff = (shoulder - hip).normalized;

        Vector2 hand = Vector2.Lerp(pGraphics.hands[0 + index].lastPos, pGraphics.hands[0 + index].pos, timeStacker)
            + 2 * shoulder_hip_diff;


        /////////////////////////////////////
        // Inbetween points
        /////////////////////////////////////


        // (All Vector2s initalized with the type "var" are not vertices)
        var hand_shoulder_dist = 0.1f * Vector2.Distance(hand, shoulder);

        var hand_shoulder_diff = index == 0 ? hand - shoulder : shoulder - hand;


        Vector2 hand_shoulder_inbetween = Vector2.Lerp(shoulder, hand, 0.5f);


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

        ((TriangleMesh)sLeaser.sprites[startSprite]).MoveVertice(0, hand - camPos);
        ((TriangleMesh)sLeaser.sprites[startSprite]).MoveVertice(1, hand_to_inbetween - camPos);
        ((TriangleMesh)sLeaser.sprites[startSprite]).MoveVertice(2, hand_shoulder_inbetween - camPos);

        ((TriangleMesh)sLeaser.sprites[startSprite]).MoveVertice(3, hips_to_inbetween - camPos);
        ((TriangleMesh)sLeaser.sprites[startSprite]).MoveVertice(4, shoulder - camPos);
        ((TriangleMesh)sLeaser.sprites[startSprite]).MoveVertice(5, hip - camPos);
    }

    public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    { }

    public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer? newContainer)
    {
        for (int i = 0; i < 2; i++)
        {
            newContainer ??= rCam.ReturnFContainer("Midground");
            newContainer.AddChild(sLeaser.sprites[startSprite + i]);

            sLeaser.sprites[startSprite + i].MoveBehindOtherNode(sLeaser.sprites[0]);
        }
    }
}
