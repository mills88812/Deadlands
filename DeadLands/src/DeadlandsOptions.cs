using Menu.Remix.MixedUI;
using UnityEngine;
using UnityEngine.UIElements.Experimental;

namespace Deadlands
{
    public class DeadlandsOptions : OptionInterface
    {
        public readonly Configurable<KeyCode> GlideKey;
        public readonly Configurable<float> Num;
        private UIelement[] UIArrOptions;

        public DeadlandsOptions()
        {
            GlideKey = this.config.Bind<KeyCode>("Glide", new KeyCode());
            Num = this.config.Bind<float>("", 0.12f);
        }

        public override void Initialize()
        {
            var opTab = new OpTab(this, "Options");
            this.Tabs = new[] { opTab };

            OpContainer tab1Container = new(new Vector2(0, 0));
            opTab.AddItems(tab1Container);

            var offset = 75f;
            UIelement[] UIArrayElements1 = new UIelement[]
            {
                new OpLabel(10f, 450f - offset, "Options", true),

                new OpLabel(170f, 450f - offset, "Glide Keybind"),
                new OpKeyBinder(GlideKey, new Vector2(10f, 420f - offset), new Vector2(150f, 30f), true, OpKeyBinder.BindController.AnyController),// { description = Translate("Set up you glide keybind for Nomad") },
            };
            opTab.AddItems(UIArrayElements1);
        }
    }
}