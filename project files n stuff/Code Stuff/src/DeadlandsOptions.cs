namespace Deadlands;

public class DeadlandsOptions : OptionInterface
{
    public DeadlandsOptions()
    {
        GlideKey = config.Bind("Glide", new KeyCode());
        Num = config.Bind("", 0.12f);
    }

    public readonly Configurable<KeyCode> GlideKey;
    public readonly Configurable<float> Num;

    public override void Initialize()
    {
        var opTab = new OpTab(this, "Options");
        Tabs = [opTab];

        OpContainer tab1Container = new(new Vector2(0, 0));
        opTab.AddItems(tab1Container);

        var offset = 75f;
        UIelement[] UIArrayElements1 =
        [
            new OpLabel(10f, 450f - offset, "Options", true),

            new OpLabel(170f, 450f - offset, "Glide Keybind"),
            new OpKeyBinder(GlideKey, new Vector2(10f, 420f - offset), new Vector2(150f, 30f), true, OpKeyBinder.BindController.AnyController),
        ];
        opTab.AddItems(UIArrayElements1);
    }
}