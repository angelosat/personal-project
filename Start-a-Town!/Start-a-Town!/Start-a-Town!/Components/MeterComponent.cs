using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Start_a_Town_.UI;

namespace Start_a_Town_.Components
{
    class MeterComponent : Component
    {
        Bar Bar;
        Position Position;

        public MeterComponent(GameObject obj, Progress progress)
        {
            Position = obj.GetComponent<MovementComponent>("Position").GetProperty<Position>("Position");
            Bar bar = new Bar(new Vector2(Game1.Instance.graphics.PreferredBackBufferWidth / 2.0f, Game1.Instance.graphics.PreferredBackBufferHeight / 4.0f));
            bar.Location.X -= bar.Center.X;
            bar.Track(progress);
            bar.Object = obj;
            //bar.Text = "Press " + GlobalVars.KeyBindings.Use;
         //   bar.Show();
        }

        public override object Clone()
        {
            return new MeterComponent(Bar.Object, Bar.Meter);
        }
    }
}
