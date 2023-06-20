using DressMySlugcat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Deadlands
{
    internal class NomadDMScompatibility
    {
        public static void OnInit()
        {
            SetupDMSSprites();
        }

        public static void SetupDMSSprites()
        {
            for (int i = 0; i < 4; i++)
            {
                SpriteDefinitions.AddSlugcatDefault(new Customization()
                {
                    Slugcat = "Nomad",
                    PlayerNumber = i,
                    CustomSprites = new List<CustomSprite>
                    {
                        new CustomSprite() { Sprite = "HEAD", SpriteSheetID = "Perle_gel.Nomad" },
                        new CustomSprite() { Sprite = "FACE", SpriteSheetID = "Perle_gel.Nomad", Color = Color.black },
                        new CustomSprite() { Sprite = "BODY", SpriteSheetID = "Perle_gel.Nomad" },
                        new CustomSprite() { Sprite = "ARMS", SpriteSheetID = "Perle_gel.Nomad" },
                        new CustomSprite() { Sprite = "HIPS", SpriteSheetID = "Perle_gel.Nomad" },
                        new CustomSprite() { Sprite = "TAIL", SpriteSheetID = "Perle_gel.Nomad" }
                    }
                });
            }
        }
    }
}
