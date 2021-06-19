using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Start_a_Town_
{
    class Size
    {
        public int Width, Height;
        public Size(int w, int h)
        {
            Width = w;
            Height = h;
        }
        public Size(int d)
        {
            Width = d;
            Height = d;
        }
        public Size()
        { }

        public Rectangle ToRectangle()
        {
            return new Rectangle(0, 0, Width, Height);
        }
        public Vector2 ToVector2()
        {
            return new Vector2(Width, Height);
        }
    }
}
