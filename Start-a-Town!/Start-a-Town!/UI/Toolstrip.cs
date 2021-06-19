using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Start_a_Town_.UI
{
    class Toolstrip : Control
    {
        public ToolstripItemCollection Items { get; set; }

        public override void Draw(SpriteBatch sb)
        {
            base.Draw(sb);
        }
    }
}
