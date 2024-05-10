namespace Deadlands;

public static class Utils
{
    public static Vector2 Abs(this Vector2 vec)
        => new(Mathf.Abs(vec.x), Mathf.Abs(vec.y));

    public static void UnregisterEnums(Type type)
    {
        var extEnums = type.GetFields(BindingFlags.Static | BindingFlags.Public).Where(x => x.FieldType.IsSubclassOf(typeof(ExtEnumBase)));

        foreach (var extEnum in extEnums)
        {
            var obj = extEnum.GetValue(null);
            if (obj != null)
            {
                obj.GetType().GetMethod("Unregister")!.Invoke(obj, null);
                extEnum.SetValue(null, null);
            }
        }
    }

    /// <summary>
    /// Returns a small mesh, consisting of two quads. The vertices' positions can be manipulated to control where a sprite goes.
    /// </summary>
    public static TriangleMesh CreateSimpleMesh()
    {
        // All indices in these Triangle ctors are sorted clock-wise (Probably doesn't matter cause it's a 2D game, but I did it anyways)
        /*
        0 ------ 2 ------ 4
        |       /|       /|
        |     /  |     /  |
        |   /    |   /    |
        | /      | /      |
        1 ------ 3 ------ 5
        */
        return new TriangleMesh("Futile_White",
            [
                new(0, 2, 1),
                new(1, 2, 3),
                new(2, 4, 3),
                new(3, 4, 5)
            ], false);
    }
}