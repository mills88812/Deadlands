using UnityEngine.Assertions;

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
    private const int _whiskerCount = 2;

    public const int RequiredSprites = _whiskerCount;

    public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        Assert.IsTrue(_whiskerCount % 2 == 0); // Has to be an even number of whiskers.
        for (int i = 0; i < RequiredSprites; i++)
        {
            sLeaser.sprites[_startSprite + i] = Utils.CreateSimpleMesh();
        }
    }

    public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        sLeaser.sprites[_startSprite].color = Color.green;
        sLeaser.sprites[_startSprite + 1].color = Color.magenta;
        var playerHead = _pGraphics.player.firstChunk.pos;
        for (int i = 0; i < RequiredSprites; i++)
        {
            var mesh = sLeaser.sprites[_startSprite + i] as TriangleMesh;
            Assert.IsNotNull(mesh);

            // Various moustache properties (all measured in pixels)
            const int offset = 3; // how far we are from the nose
            const int lengthBend = 5; // the horizontal girth of the bent part
            const int lengthStraight = 3; // the girth of the straightaway part
            const int height = 5; // the height
            int verticalOrigin = i * 3 * height; // How up we are from the nose (differs between whiskers)

            mesh.MoveVertice(0, playerHead + new Vector2(-lengthStraight - offset - lengthBend, verticalOrigin));
            mesh.MoveVertice(1, playerHead + new Vector2(-lengthStraight - offset - lengthBend*0.9f, verticalOrigin - height*0.9f));
            mesh.MoveVertice(2, playerHead + new Vector2(-lengthStraight - offset, height + verticalOrigin));
            mesh.MoveVertice(3, playerHead + new Vector2(-lengthStraight - offset, verticalOrigin));
            mesh.MoveVertice(4, playerHead + new Vector2(-offset, height + verticalOrigin));
            mesh.MoveVertice(5, playerHead + new Vector2(-offset, verticalOrigin));
        }
    }

    public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        // Just do whatever the root node is doing
        for (int i = 0; i < RequiredSprites; i++)
            sLeaser.sprites[_startSprite + i].color = sLeaser.sprites[0].color;
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