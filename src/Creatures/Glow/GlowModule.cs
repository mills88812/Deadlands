using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RWCustom;
using System.Numerics;
using System.Reflection;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;
using Random = UnityEngine.Random;
using System.Globalization;
using System.Text.RegularExpressions;
using static MonoMod.InlineRT.MonoModRule;

namespace Deadlands.Creatures.Glow
{
    internal class GlowModule
    {

        
        public Lizard owner;


        
        public LightSource light;
        public LizardGraphics graphics
        {
            get
            {
                return owner.graphicsModule as LizardGraphics;
            }
        }
        

        public LizardAI AI
        {
            get
            {
                return owner.AI as LizardAI;
            }
        }

        public GlowModule(Lizard lizard)
        {
            owner = lizard;
        }
        public void UpdateGraphics(LizardGraphics self)
        {


            float lightMode = 0;
            if (owner.animation == Lizard.Animation.ThreatReSpotted || owner.animation == Lizard.Animation.PreyReSpotted || owner.animation == Lizard.Animation.PreySpotted || owner.animation == Lizard.Animation.FightingStance || owner.animation == Lizard.Animation.ThreatSpotted)
            {
                lightMode += 140;
            }
            if (owner.animation == Lizard.Animation.HearSound)
            {
                lightMode += 100;
            }
            if (owner.Stunned)
            {

                lightMode -= 10;

            }
            if (owner.dead)
            {

                lightMode -= 100;
            }

            if (!owner.dead)
            {

                lightMode = Mathf.Lerp(lightMode, 5f, 1f / 20f);

            }
            if (self.lightSource != null)
            {
                self.lightSource.stayAlive = true;
                self.lightSource.setPos = self.head.pos;
                self.lightSource.setRad = Mathf.Lerp(Mathf.Lerp(160f, 560f, lightMode * (100f + self.flicker)), (lightMode) + 200f, 800f);
                self.lightSource.setAlpha = Mathf.Lerp(Mathf.Lerp(0.8f, 0.3f, lightMode), 2f, Custom.SCurve(1f, 0.3f) * 0.5f) * 0.9f * (1f + self.flicker * 0.4f);
                self.lightSource.color = Custom.HSL2RGB(Custom.WrappedRandomVariation(.06f, .03f, .66f), 1f, Custom.ClampedRandomVariation(0.6f, .03f, 0.9f));
                if (self.lightSource.slatedForDeletetion || owner.room.Darkness(self.head.pos) == 0f)
                {
                    self.lightSource = null;
                }
            }
            else if (owner.room.Darkness(self.head.pos) > 0f)
            {
                self.lightSource = new LightSource(self.head.pos, false, new Color(1f, 1f, 1f), self.lizard);
                self.lightSource.requireUpKeep = true;
                owner.room.AddObject(self.lightSource);
            }


            for (int num99 = 0; num99 < self.tail.Length; num99++)
            {

                if (light != null)
                {
                    light.stayAlive = true;
                    light.setPos = self.tail[4].pos;
                    light.setRad = Mathf.Lerp(Mathf.Lerp(160f, 560f, lightMode / 3 * (100f + self.flicker)), (lightMode / 3) + 200f, 800f);
                    light.setAlpha = Mathf.Lerp(Mathf.Lerp(0.8f, 0.3f, lightMode / 3), 2f, Custom.SCurve(1f, 0.3f) * 0.5f) * 0.9f * (1f + self.flicker * 0.4f);
                    if (light.slatedForDeletetion || owner.room.Darkness(self.tail[4].pos) == 0f)
                    {
                        light = null;
                    }
                    if (owner.animation == Lizard.Animation.ThreatReSpotted || owner.Stunned || owner.animation == Lizard.Animation.ThreatSpotted)
                    {

                        light.color = new Color(Random.value, Random.value, Random.value);

                    }
                    else
                        light.color = Custom.HSL2RGB(Custom.WrappedRandomVariation(.06f, .03f, .66f), 1f, Custom.ClampedRandomVariation(0.6f, .03f, 0.9f));
                }
                else if (owner.room.Darkness(self.tail[4].pos) > 0f)
                {
                    light = new LightSource(self.tail[4].pos, false, new Color(1f, 1f, 1f), self.lizard);
                    light.requireUpKeep = true;
                    owner.room.AddObject(light);
                }
            }
        }
    }
}
