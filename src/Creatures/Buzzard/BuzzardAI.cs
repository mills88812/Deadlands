using Deadlands.Hooks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Deadlands.Creatures.Buzzard
{

    /// <summary>
    /// Extension of VultureAI with ItemTracking capabilities used for the Buzzard
    /// </summary>
    internal class BuzzardAI : VultureAI, IUseItemTracker
    {

        private BuzzardModule Module
        {
            get {
                BuzzardHooks.BuzzardModule.TryGetValue(vulture, out var value);
                return value;
            }
        }

        public BuzzardAI(AbstractCreature creature, World world) : base(creature, world)
        {
            base.AddModule(new ItemTracker(this, 10, 15, 600, 4000, true));
        }

        public void SeeThrownWeapon(PhysicalObject obj, Creature thrower)
        {
        }

        public bool TrackItem(AbstractPhysicalObject obj)
        {
            return obj.realizedObject != null && obj.realizedObject is Weapon;
        }

        public void GrabObject(PhysicalObject obj)
        {
            if (Module.wantToGrabChunk == null && Module.grabChunk == null && this.vulture.snapAt == null)
            {
                Module.wantToGrabChunk = obj.firstChunk;
            }
        }
    }
}
