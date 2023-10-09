using UnityEngine;

namespace Deadlands;

internal static class HelperFuncs
{
    public static Vector2 Abs(this Vector2 vec)
        => new(Mathf.Abs(vec.x), Mathf.Abs(vec.y));
}
