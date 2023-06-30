using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Steamworks;
using UnityEngine;

namespace Deadlands;

public static class HydrationLogic
{
    private const float MaxHydration = 20f;

    private static ConditionalWeakTable<Player, StrongBox<float>> HydrationData = new();

    public static float GetHydration(this Player player) =>
        HydrationData.TryGetValue(player, out var strongBox) ? strongBox.Value : MaxHydration;

    public static void AddHydration(this Player player, float value)
    {
        if (HydrationData.TryGetValue(player, out var strongBox))
        {
            strongBox.Value += value;
        }
        Debug.Log("HYDRATION: "+GetHydration(player));
    }

    public static void SetupHydration()
    {
        On.Player.ThrowObject += (orig, self, grasp, eu) =>
        {
            orig(self, grasp, eu);
        };

        new WaterValuesSetup();
        On.Player.ctor += (orig, self, creature, world) =>
        {
            orig(self, creature, world);

            HydrationData.Add(self, new StrongBox<float>());
        };

        On.Player.Jump += (orig, self) =>
        {
            orig(self);
            self.AddHydration(self.GetHydration()-5);
        };

        On.Player.ObjectEaten += (orig, self, edible) =>
        {
            orig(self, edible);
            Type edibletype = edible.GetType();
            Debug.Log($"Ate {edibletype.Name}");
            self.AddHydration(WaterValues.GetFoodValue(edibletype).BaseWater);
        };
    }
}