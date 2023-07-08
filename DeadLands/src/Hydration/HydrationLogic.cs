using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Deadlands;

public static class HydrationLogic
{
    private const int MaxHydration = 15; //TODO make max hydration different for each slugs + add possibility for modded slugs to add their hydration as well

    private static ConditionalWeakTable<Player, StrongBox<int>> HydrationData = new();

    public static int GetHydration(this Player player) =>
        HydrationData.TryGetValue(player, out var strongBox) ? strongBox.Value : MaxHydration;

    public static void AddHydration(this Player player, int value)
    {
        if (HydrationData.TryGetValue(player, out var strongBox))
        {
            strongBox.Value += value;
        }
        Debug.Log("HYDRATION: "+GetHydration(player));
    }

    public static void SetupHydration()
    {
        HydrationMeter.AddToHud(MaxHydration);
        
        On.Player.ThrowObject += (orig, self, grasp, eu) =>
        {
            orig(self, grasp, eu);
        };

        new WaterValuesSetup();
        On.Player.ctor += (orig, self, creature, world) =>
        {
            orig(self, creature, world);

            HydrationData.Add(self, new (MaxHydration));
        };

        On.Player.Jump += (orig, self) =>
        {
            orig(self);
            self.AddHydration(-1);
        };

        On.Player.ObjectEaten += (orig, self, edible) =>
        {
            orig(self, edible);
            Type edibletype = edible.GetType();
            Debug.Log($"Ate {edibletype.Name}");
            //self.AddHydration(WaterValues.GetFoodValue(edibletype).BaseWater);
        };
    }
}