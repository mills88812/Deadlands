using System.Runtime.CompilerServices;

namespace Deadlands.Nomad;

internal static class Nomad
{
    public static SlugcatStats.Name Name = new(nameof(Nomad));

    public static readonly ConditionalWeakTable<Player, NomadData> NomadData = new();

    // Not sure if this is even needed
    public static void PluginCtor()
    {
        /*
        Name = new(nameof(Nomad), true);
        
        if (ExtEnumBase.TryParse(typeof(SlugcatStats.Name), "Nomad", true, out var extEnum))
        {
            Name = (extEnum as SlugcatStats.Name)!;
        }
        */
        
    }
    
    public static void OnInit()
    {
        On.Player.ctor += (orig, self, creature, world) =>
        {
            orig(self, creature, world);

            if (!Plugin.Nomad.TryGet(self, out var nomad) || !nomad) return;
            
            
            NomadData.Add(self, new NomadData(self));
            
            if (self.room.world.game.IsArenaSession) return;

            ((PlayerState)self.State).slugcatCharacter = Name;
            self.SlugCatClass = Name;
        };
        
        NomadGliding.OnInit();
        NomadGraphics.OnInit();
    }
}