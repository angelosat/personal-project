using System;
using System.Collections.Generic;
using System.IO;
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
        public static Color Add(this Color c1, Vector4 c2)
        {
            return new Color(c1.R + c2.X * 255, c1.G + c2.Y * 255, c1.B + c2.Z * 255, c1.A + c2.W * 255);
        }
        public static Color Add(this Color c1, Color c2)
        {
            return new Color(c1.R + c2.R, c1.G + c2.G, c1.B + c2.B, c1.A + c2.A);
        }
        public static Color Multiply(this Color c1, Color c2)
        {
            float r = (c1.R / 255f) * (c2.R / 255f);
            float g = (c1.G / 255f) * (c2.G / 255f);
            float b = (c1.B / 255f) * (c2.B / 255f);
            float a = (c1.A / 255f) * (c2.A / 255f);
            return new Color(r, g, b, a);
        }

        public static List<SaveTag> Save(this Color c)
        {
            var tag = new List<SaveTag>
            {
                new SaveTag(SaveTag.Types.Byte, "R", c.R),
                new SaveTag(SaveTag.Types.Byte, "G", c.G),
                new SaveTag(SaveTag.Types.Byte, "B", c.B),
                new SaveTag(SaveTag.Types.Byte, "A", c.A)
            };
            return tag;
        }

        static public Color GetColor(this Random rand)
        {
            var array = new byte[3];
            rand.NextBytes(array);
            return new Color(array[0], array[1], array[2]);
        }
        public static void Write(this Color c, BinaryWriter w)
        {
            w.Write(c.R);
            w.Write(c.G);
            w.Write(c.B);
            w.Write(c.A);
        }
        public static void Write(this BinaryWriter w, Color c)
        {
            w.Write(c.R);
            w.Write(c.G);
            w.Write(c.B);
            w.Write(c.A);
        }
        public static Color ReadColor(this BinaryReader r)
        {
            var c = new Color
            {
                R = r.ReadByte(),
                G = r.ReadByte(),
                B = r.ReadByte(),
                A = r.ReadByte()
            };
            return c;
        }
    }
}
