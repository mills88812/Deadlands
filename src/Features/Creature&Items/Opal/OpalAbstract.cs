using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Fisobs.Core;

namespace DeadlandsCreatures.Features.Opal
{
    public class OpalAbstract : AbstractPhysicalObject
    {
        public OpalAbstract(World world, WorldCoordinate pos, EntityID ID) : base(world, OpalCritob.AbstrOpal, null, pos, ID)
        {
            scaleX = 2;
            scaleY = 2;
            saturation = 0.9f;
            hue = 1f;
            
        }

        public override void Realize()
        {
            base.Realize();
            if (realizedObject == null)
                realizedObject = new Opal(this, this);
        }

        public float hue;
        public float saturation;
        public float scaleX;
        public float scaleY;

        public override string ToString()
        {
            return this.SaveToString($"{hue};{saturation};{scaleX};{scaleY}");
        }
    }
}