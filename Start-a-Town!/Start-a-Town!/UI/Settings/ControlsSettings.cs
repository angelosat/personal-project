using System.Linq;
using Microsoft.Xna.Framework;
using Start_a_Town_.UI;

namespace Start_a_Town_
{
    class ControlsSettings : GameSettings
    {
        GroupBox _Gui;
        internal GroupBox Gui => this._Gui ??= this.CreateGui();

        GroupBox CreateGui()
        {
            var box = new GroupBox();
            box.Name = "Controls";
            var w = (from f in PlayerInput.KeyBindings select f.Key.ToString()).MaxWidth(UIManager.Font);
            var btnw = (from f in PlayerInput.KeyBindings select f.Value.ToString()).MaxWidth(UIManager.Font);

            foreach(var key in PlayerInput.KeyBindings)
            {
                var label = new Label(key.Key.ToString()) { Location = box.Controls.BottomLeft };
                var button = new Button(key.Value.ToString(), btnw) { Location = new Vector2(w, label.Location.Y) };
                box.Controls.Add(label, button);
            }
            return box;
        }

        internal override void Apply()
        {
        }
    }
}
