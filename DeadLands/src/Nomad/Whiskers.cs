using UnityEngine;
using RWCustom;

namespace Deadlands.Nomad;

sealed class Whiskers
{
    private readonly PlayerGraphics _pGraphics;

    private readonly int _startSprite;
    
    private readonly float _length = 0.12f;
    
    public Whiskers(PlayerGraphics owner, int startSprite, float length = 0.12f)
    {
        this._pGraphics = owner;
        this._startSprite = startSprite;

        this._length = length;
    }

    public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        sLeaser.sprites[_startSprite] = TriangleMesh.MakeLongMesh(5, false, false); // new FSprite("Circle20");
    }


    public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        var playerHead = 
            Vector2.Lerp(_pGraphics.player.firstChunk.lastPos, _pGraphics.player.firstChunk.pos, timeStacker);
        var playerHips = 
            Vector2.Lerp(_pGraphics.player.bodyChunks[1].lastPos, _pGraphics.player.bodyChunks[1].pos, timeStacker);
        var playerTail =
            playerHips + (playerHead - playerHips);

        var hipsHeadDiff = Custom.DirVec(playerHips, playerHead);

        /*
        sLeaser.sprites[_startSprite].x = playerHead.x - camPos.x;
        sLeaser.sprites[_startSprite].y = playerHead.y - camPos.y;
        */

        for (int i = 0; i < 5; i++)
        {
            float f = Mathf.InverseLerp(0f, 6f, i);

            Vector2 bezierPoint =
                Custom.Bezier(playerHead + hipsHeadDiff * 3f, playerHips, playerTail, playerHips, f);


            var mesh = sLeaser.sprites[_startSprite] as TriangleMesh;

            mesh.MoveVertice(i * 4, bezierPoint + Vector2.up * 100 - camPos);
            mesh.MoveVertice(i * 4 + 1, bezierPoint + Vector2.up * 100 + Vector2.right * 3 - camPos);
            mesh.MoveVertice(i * 4 + 2, bezierPoint + Vector2.up * 100 + Vector2.up * 3 - camPos);
            mesh.MoveVertice(i * 4 + 3, bezierPoint + Vector2.up * 100 + (Vector2.right * 3) + (Vector2.right * 3) - camPos);
        }
    }

    public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        sLeaser.sprites[0].color = Color.white;
    }

    public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer? newContainer)
    {
        newContainer ??= rCam.ReturnFContainer("Midground");
        newContainer.AddChild(sLeaser.sprites[_startSprite]);

        sLeaser.sprites[_startSprite].MoveToFront();
    }
}
