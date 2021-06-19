using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Start_a_Town_
{
    static class CameraGlobal
    {
        public static Vector2 ScreenLocation;
        public static int width, height;

        public static Vector2 getScreenLocation()
        {
            return ScreenLocation;
        }

        public static void setScreenLocation(Vector2 pos)
        {
            ScreenLocation = pos;
        }

        public static int getWidth()
        {
            return width;
        }

        public static int getHeight()
        {
            return height;
        }

        public static void setSize(int w, int h)
        {
            width = w;
            height = h;
        }
    }
}
