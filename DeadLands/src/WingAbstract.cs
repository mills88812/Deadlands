using Fisobs.Core;
using UnityEngine;

namespace Deadlands;

sealed class WingAbstract : AbstractPhysicalObject
{
    public WingAbstract(World world, WorldCoordinate pos, EntityID ID) : base(world, WingFisob.Wing, null, pos, ID)
    { }

    public override void Realize()
    {
        base.Realize();
        if (realizedObject == null)
            realizedObject = new Wing(this, Room.realizedRoom.MiddleOfTile(pos.Tile), Vector2.zero);
    }

    

    public override string ToString()
    {
        return this.SaveToString($"");
    }
}
