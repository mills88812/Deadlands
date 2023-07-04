//using DressMySlugcat;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Deadlands
{
    internal class NomadDMScompatibility
    {
        public static void OnInit()
        {
            //SetupDMSSprites();
        }

        /*private static void SetupDMSSprites()
        {
            for (int i = 0; i < 4; i++)
            {
                SpriteDefinitions.AddSlugcatDefault(new Customization()
                {
                    Slugcat = "Nomad",
                    PlayerNumber = i,
                    CustomSprites = new List<CustomSprite>
                    {
                        new() { Sprite = "HEAD", SpriteSheetID = "Perle_gel.Nomad" },
                        new() { Sprite = "FACE", SpriteSheetID = "Perle_gel.Nomad", Color = Color.black },
                        new() { Sprite = "BODY", SpriteSheetID = "Perle_gel.Nomad" },
                        new() { Sprite = "ARMS", SpriteSheetID = "Perle_gel.Nomad" },
                        new() { Sprite = "HIPS", SpriteSheetID = "Perle_gel.Nomad" },
                        new() { Sprite = "TAIL", SpriteSheetID = "Perle_gel.Nomad" }
                    }
                });
            }
        }*/
    }
}
