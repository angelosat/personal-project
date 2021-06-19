using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.PlayerControl;

namespace Start_a_Town_.UI.Settings
{
    class ControlsSettings : GroupBox
    {
        public ControlsSettings()
        {
            this.Name = "Controls";
            var w = (from f in PlayerInput.KeyBindings select f.Key.ToString()).MaxWidth(UIManager.Font);
            var btnw = (from f in PlayerInput.KeyBindings select f.Value.ToString()).MaxWidth(UIManager.Font);

            foreach(var key in PlayerInput.KeyBindings)
            {
                var label = new Label(key.Key.ToString()) { Location = this.Controls.BottomLeft };
                var button = new Button(key.Value.ToString(), btnw) { Location = new Vector2(w, label.Location.Y) };
                this.Controls.Add(label, button);
            }
        }
    }
}
