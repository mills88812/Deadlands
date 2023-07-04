using Menu.Remix.MixedUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements.Experimental;

namespace Deadlands
{
    public class DeadlandsOptionsMenu : OptionInterface
    {
        public static Configurable<KeyCode> Glide { get; set; }

        public DeadlandsOptionsMenu()
        {
            Glide = config.Bind("Glide", new KeyCode());
        }

        public override void Initialize()
        {
            OpKeyBinder.BindController controllerNumber = OpKeyBinder.BindController.AnyController;

            var opTab1 = new OpTab(this, "Options");
            Tabs = new[] { opTab1 };

            OpContainer tab1Container = new(new Vector2(0, 0));
            opTab1.AddItems(tab1Container);

            UIelement[] UIArrayElements1 = new UIelement[]
            {
                new OpLabel(0f, 580f, "Options", true),

                new OpKeyBinder(Glide, new Vector2(10,530),new Vector2(150,10), false, controllerNumber) { description = Translate("Set up you glide keybind for Nomad") },
                new OpLabel(170f, 530f, Translate("Glide Keybind")),

            };
            opTab1.AddItems(UIArrayElements1);
        }

    }
}