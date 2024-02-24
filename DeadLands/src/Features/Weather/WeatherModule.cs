using Deadlands.Hooks.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Deadlands.Features
{
    /// <summary>
    /// Global WeatherModule responsible for selecting weather to happen in the Deadlands
    /// </summary>
    internal class WeatherModule
    {
        public bool STASIS = true;

        public int cyclesSinceLastRain;
        public float globalSandstormProgress;
        public WeatherModule() 
        {

        }

        public void Update()
        {
            if (STASIS)
            {
                return;
            }
        }

        public bool isActive()
        {
            return !STASIS;
        }
    }
}
