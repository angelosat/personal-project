using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.UI
{
    static class ButtonHelper
    {
        static public Button CreateFromItemCompact(Entity obj)
        {
            var btn = new Button();
            //var padding = btn.BackgroundStyle.Left.Width;
            var pic = new PictureBox(obj, .5f) { MouseThrough = true };//, Location = new Vector2(btn.Padding, btn.Height / 2), Anchor = new Vector2(0, .5f) };
            var lbl = new Label(obj.Name) { MouseThrough = true, Location = new Vector2(pic.Right, btn.Height / 2) };//, Anchor = new Vector2(0, .5f) };
            lbl.Location.Y -= lbl.Height / 2;
            btn.AddControls(pic, lbl);
            return btn;
        }
    }
}
