using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Start_a_Town_.UI
{
    class ColoredRectangle : ButtonBase
    {
        public ColoredRectangle(int width, int height)
        {
            this.Width = width;
            this.Height = height;
        }

        public override void Draw(SpriteBatch sb, Rectangle viewport)
        {
            this.DrawHighlight(sb, this.ScreenBounds, this.Tint);
        }
    }
}
