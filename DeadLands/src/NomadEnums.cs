using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Deadlands
{
    internal class NomadEnums
    {
        public static SoundID wind { get; set; }
        //Hook to plugin cs
        public static void RegisterValues()
        {
            wind = new SoundID("wind", true);
        }
        //DO NOT
        public static void UnregisterValues()
        {
            Unregister(wind);
        }
        //Do...
        private static void Unregister<T>(ExtEnum<T> extEnum) where T : ExtEnum<T>
        {
            extEnum?.Unregister();
        }
    }
}
