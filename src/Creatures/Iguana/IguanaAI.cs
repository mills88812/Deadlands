using Deadlands.Hooks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Deadlands.Features.Opal;


namespace Deadlands.Creatures.Iguana
{
    internal class IguanaAI : LizardAI
    {
        public class Behavio : ExtEnum<Behavior>
        {
        

            public static readonly Behavior GoToFood = new("GoToFood", register: true);

            


            public Behavio(string value, bool register = false)
                : base(value, register)
            {
            }
        }
        private IguanaModule Module
        {
            get
            {
                IguanaHook.IguanaModule.TryGetValue(lizard, out var value);
                return value;
            }
        }
        public IguanaAI(AbstractCreature creature, World world) : base(creature, world)
        {

        }

       
        public PhysicalObject Food;
        public override void Update()
        {
            if (Food != null)
        {
            if (!WantToEatFruts(Food))
            {
                    Food = null;
            }
            else if (currentUtility< 0.75f && lizard.grasps[0] == null)
            {
                currentUtility = 0.75f;
                behavior = Behavio.GoToFood;
            }
            }
            if (behavior == Behavio.GoToFood)
                {
                    creature.abstractAI.SetDestination(lizard.room.GetWorldCoordinate(Food.firstChunk.pos));
                }
            
            base.Update();
        }
        public bool WantToEatFruts(PhysicalObject obj)
        {
            if (obj == null)
            {
                return false;
            }

            if (!(obj is DangleFruit) && !(obj is SwollenWaterNut) && !(obj is Opal) && !(obj is SlimeMold))
            {
                return false;
            }

            if (obj.room != null && obj.room == lizard.room && obj.grabbedBy.Count == 0 && !obj.slatedForDeletetion && (base.pathFinder.CoordinateReachableAndGetbackable(lizard.room.GetWorldCoordinate(obj.firstChunk.pos)) || base.pathFinder.CoordinateReachableAndGetbackable(lizard.room.GetWorldCoordinate(obj.firstChunk.pos) ) || base.pathFinder.CoordinateReachableAndGetbackable(lizard.room.GetWorldCoordinate(obj.firstChunk.pos) )))
            {
                return base.threatTracker.ThreatOfArea(lizard.room.GetWorldCoordinate(obj.firstChunk.pos), accountThreatCreatureAccessibility: true) < 0.55f;
            }

            return false;
        }
    }
}
