using Fisobs.Core;
using Fisobs.Items;
using Fisobs.Properties;
using Fisobs.Sandbox;

namespace Deadlands;

sealed class WingFisob : Fisob
{
    public static readonly AbstractPhysicalObject.AbstractObjectType Wing = new("Wing", true);

    public WingFisob() : base(Wing)
    { }

    public override AbstractPhysicalObject Parse(World world, EntitySaveData saveData, SandboxUnlock? unlock)
    {
        var result = new WingAbstract(world, saveData.Pos, saveData.ID) {};

        return result;
    }

    private static readonly WingProperties properties = new();

    public override ItemProperties Properties(PhysicalObject forObject)
    {
        return properties;
    }
}

sealed class WingProperties : ItemProperties
{
    public override void Grabability(Player player, ref Player.ObjectGrabability grabability)
    {
        grabability = Player.ObjectGrabability.CantGrab;
    }
}
