using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Start_a_Town_
{
    public class TileSprite
    {
        public Rectangle[][] SourceRects;
        public Rectangle[][] Highlights;
        public Vector2 Origin;
        public MouseMap MouseMap;
        public string Name;

        public TileSprite(string name, Rectangle[][] sourceRects, Vector2 origin, MouseMap mouseMap, Rectangle[][] highlights)
        {
            Name = name;
            SourceRects = sourceRects;
            Origin = origin;
            MouseMap = mouseMap;
            Highlights = highlights;
        }

        public Rectangle GetBounds()
        {
            Rectangle rect = SourceRects[0][0];
            return new Rectangle((int)-Origin.X, (int)-Origin.Y, rect.Width, rect.Height);
        }

        public int GetFace(Color color)
        {
            //if (color.A == 0)
            //    return null;
            int face = 0;
            if (color.R == 255)
                face = 1;
            else if (color.G == 255)
                face = (byte)(2 + 2 * (int)(8 * ((color.B + 1) / (float)(255 + 1))));
            else if (color.B == 255)
                face = (byte)(3 + 2 * (int)(8 * ((color.G + 1) / (float)(255 + 1))));
            return face;
        }
    }
}
