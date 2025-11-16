using Deadlands;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Deadlands.Hooks
{
    internal class SaltWormHooks
    {

        

        public static void Apply()
        {
            // Centipede

            On.Centipede.ctor += OnCentipedeCtor;

            On.Centipede.GenerateSize += OnGenerateSize;

            // CentipedeAI

            On.CentipedeAI.ctor += OnCentipedeAICtor;

            // CentipedeGraphics

            On.CentipedeGraphics.WhiskerLength += WhiskerLength;

            On.CentipedeGraphics.ctor += OnCentipedeGraphicsCtor;
        }

        #region Centipede

        private static float OnGenerateSize(On.Centipede.orig_GenerateSize orig, AbstractCreature abstrCrit)
        {
            if (abstrCrit.creatureTemplate.type == DLCreature.SaltWorm)
            {
                Random.State state = Random.state;
                Random.InitState(abstrCrit.ID.RandomSeed);
                float result = result = Mathf.Lerp(0.9f, 1.7f, Random.value);
                Random.state = state;
                if (abstrCrit.spawnData != null && abstrCrit.spawnData.Length > 2)
                {
                    string s = abstrCrit.spawnData.Substring(1, abstrCrit.spawnData.Length - 2);
                    try
                    {
                        result = float.Parse(s, NumberStyles.Any, CultureInfo.InvariantCulture);
                    }
                    catch
                    {
                    }
                }
                return result;
            } else
            {
                return orig(abstrCrit);
            }
        }

        private static void OnCentipedeCtor(On.Centipede.orig_ctor orig, Centipede self, AbstractCreature abstractCreature, World world)
        {
            orig(self, abstractCreature, world);
            for (int i = 0; i < self.bodyChunks.Length; i++)
            {
                self.bodyChunks[i].mass += 0.04f + 0.08f * Mathf.Clamp01(Mathf.Sin(Mathf.InverseLerp(0f, (float)(self.bodyChunks.Length - 1), (float)i) * 3.1415927f));
            }
        }

        #endregion

        #region CentipedeAI

        private static void OnCentipedeAICtor(On.CentipedeAI.orig_ctor orig, CentipedeAI self, AbstractCreature creature, World world)
        {
            orig(self, creature, world);
            if (self.centipede.Template.type == DLCreature.SaltWorm)
            {
                self.pathFinder.stepsPerFrame = 20;
                self.preyTracker.sureToGetPreyDistance = 120;
            }
        }

        #endregion

        #region CentipedeGraphics

        private static float WhiskerLength(On.CentipedeGraphics.orig_WhiskerLength orig, CentipedeGraphics self, int part)
        {
            if (self.centipede.Template.type == DLCreature.SaltWorm)
            {
                return 84f;
            }
            return orig(self, part);
        }

        private static void OnCentipedeGraphicsCtor(On.CentipedeGraphics.orig_ctor orig, CentipedeGraphics self, PhysicalObject ow)
        {
            orig(self, ow);
            if (self.centipede.Template.type == DLCreature.SaltWorm)
            {
                Random.State state = Random.state;
                Random.InitState(self.centipede.abstractCreature.ID.RandomSeed);
                self.wingPairs = self.centipede.bodyChunks.Length;
                self.hue = Mathf.Lerp(-0.02f, 0.01f, Random.value);
                self.saturation = 0.3f + 0.1f * Random.value;
                Random.state = state;
            }
        }

        #endregion
    }
}
