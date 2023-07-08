using System;
using System.Collections.Generic;

namespace Deadlands;

public class WaterValuesSetup
{
    public WaterValuesSetup()
    {
       /* WaterValues.AddFoodValue<DangleFruit>(new WaterInfo(5));
        WaterValues.AddFoodValue<EggBugEgg>(new WaterInfo(3));*/
    }
}

public class WaterInfo
{
    public WaterInfo(float baseWater)
    {
        BaseWater = baseWater;
    }

    public float BaseWater { get; set; }
    //maybe things like how much it gets affected by sun;
}

public static class WaterValues
{
   /* private static Dictionary<Type, WaterInfo> _values = new();

    public static WaterInfo GetFoodValue(Type edibleType)
    {
        return _values.TryGetValue(edibleType, out var value)
            ? value
            : new WaterInfo(0);
    }

    public static void AddFoodValue<T>(WaterInfo info) where T : IPlayerEdible
    {
        _values.Add(typeof(T), info);
    }*/
}