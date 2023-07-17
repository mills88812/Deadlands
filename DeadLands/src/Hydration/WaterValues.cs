using System;
using System.Collections.Generic;
using UnityEngine;

namespace Deadlands;

public class WaterValuesSetup
{
    public static Func<object, int> FixedValue(int number) => (_) => number; //function to return fixed number

    public WaterValuesSetup()
    {
        WaterInfo.AddValue<Centipede>(centipede => (int)Math.Round(centipede.size));
        WaterInfo.AddValue<DangleFruit>(FixedValue(3));
    }
}

public static class WaterInfo
{
    private static Dictionary<Type, Func<object, int>> _values = new();

    public static void AddValue<T>(Func<T, int> func) where T : IPlayerEdible
    {
        var type = typeof(T);

        if (_values.ContainsKey(type))
            Debug.Log($"Couldn't assign water value for {type}, it has already been assigned");
        else
            _values.Add(type, obj => func((T)obj)); //this line is converting obj->int into T->int
    }

    public static int GetValue<T>(T instance)
    {
        var type = typeof(T);
        return
            _values.TryGetValue(type, out var func)
                ? func(instance)
                : 0;
    }
}