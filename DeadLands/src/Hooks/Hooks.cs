using Deadlands.Hooks;

namespace Deadlands.Hooks
{
    internal class Hooks
    {
        public static void Apply()
        {
            // Core
            Core.MenuHooks.Apply();

            // World
            World.DataPearlHooks.Apply();
            World.SLOracleHooks.Apply();
            World.WorldHooks.Apply();
        }
    }
}
