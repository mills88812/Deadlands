namespace Deadlands;

public class SunShade : UpdatableAndDeletable, IDrawable
{
    private float amount;

    public SunShade(Room room)
    {
        this.room = room;
        amount = 0.003921569f;
    }

    public override void Update(bool eu)
    {
        base.Update(eu);
        if (room is null)
            return;

        foreach (var effect in room.roomSettings.effects)
        {
            if (effect.type == DeadlandsEnums.RoomEffect.SunShade)
            {
                amount = effect.amount;
            }
        }
    }

    public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
    {
        sLeaser.sprites[0].RemoveFromContainer();
        rCam.ReturnFContainer("Water").AddChild(sLeaser.sprites[0]);
    }

    public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
    }

    public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        Vector2 vector = rCam.pos - room.cameraPositions[rCam.currentCameraPosition];
        Vector2 vector2 = room.game.rainWorld.options.ScreenSize * 0.5f;
        sLeaser.sprites[0].x = vector2.x - vector.x;
        sLeaser.sprites[0].y = vector2.y - vector.y;
        sLeaser.sprites[0].alpha = amount;
        if (slatedForDeletetion || room != rCam.room)
        {
            sLeaser.CleanSpritesAndRemove();
        }
    }

    public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        sLeaser.sprites = new FSprite[1];
        sLeaser.sprites[0] = new FSprite("Futile_White", true)
        {
            anchorX = 0.5f,
            anchorY = 0.5f,
            scaleX = 100f,
            scaleY = 100f,
            color = new Color(0.003921569f, 0f, 0f),
            shader = Custom.rainWorld.Shaders["FlatLightNoFrag"]
        };
        AddToContainer(sLeaser, rCam, null);
    }
}