using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Start_a_Town_
{
    static class ColorHelper
    {
        static readonly Random ColorRand = new Random();

        static public Color GetRandomColor()
        {
            var array = new byte[3];
            ColorRand.NextBytes(array);
            return new Color(array[0], array[1], array[2]);
        }
    }
}
