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
}