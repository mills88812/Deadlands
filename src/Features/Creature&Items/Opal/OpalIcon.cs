using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Fisobs.Core;
using UnityEngine;

namespace Deadlands.Features.Opal
{
    public class OpalIcon : Icon
    {
        public override int Data(AbstractPhysicalObject apo)
        {
            return apo is OpalAbstract Opal ? (int)(Opal.hue * 1000f) : 0;
        }

        public override Color SpriteColor(int data)
        {
            return RWCustom.Custom.HSL2RGB(data / 1000f, 1000f, 1000f);
        }

        public override string SpriteName(int data)
        {
            // Fisobs autoloads the embedded resource named `icon_{Type}` automatically
            // For Crates, this is `icon_Crate`
            return "icon_Opal";
        }
    }
}