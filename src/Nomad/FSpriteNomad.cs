using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Deadlands;

/// <summary> 
/// Overrides any element modification to use the Nomad sprites instead of the normal ones. <br/>
/// This is used to (for example) replace the normal slugcat head with Nomad's head, etc etc
/// </summary>
public class FSpriteNomad : FSprite
{
    public FSpriteNomad(string elementName)
        : base(elementName)
    {
    }

    public override void HandleElementChanged()
    {
        if (!_element.name.StartsWith("nomad_"))
        {
            Debug.Log("Element name was altered to be '" + element.name + "'!");
            _element = Futile.atlasManager.GetElementWithName("nomad_" + _element.name);
            _atlas = _element.atlas;
        }
        base.HandleElementChanged();
    }
}