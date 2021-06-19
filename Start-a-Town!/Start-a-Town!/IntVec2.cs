using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Start_a_Town_
{
    public struct IntVec2 : IEquatable<IntVec2>
    {
        public int X, Y;

        public IntVec2(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }
        public IntVec2(int xy) : this(xy, xy)
        {

        }

        public IntVec2(Vector2 vector2) : this((int)Math.Round(vector2.X), (int)Math.Round(vector2.Y))
        {

        }
        public IntVec2(float x, float y) : this((int)Math.Round(x), (int)Math.Round(y))
        {

        }
        
        public bool Equals(IntVec2 other)
        {
            return
                this.X == other.X &&
                this.Y == other.Y;
        }
        public override bool Equals(object obj)
        {
            return obj is IntVec2 && this.Equals((IntVec2)obj);
        }
        public override int GetHashCode()
        {
            return new Vector2(this.X, this.Y).GetHashCode();
        }
        static public IntVec2 Zero = new IntVec2(0);
        static public IntVec2 One = new IntVec2(1);
        static public IntVec2 UnitX = new IntVec2(1, 0);
        static public IntVec2 UnitY = new IntVec2(0, 1);

        static public IntVec2 operator +(IntVec2 a, IntVec2 b)
        {
            return new IntVec2(a.X + b.X, a.Y + b.Y);
        }
        static public IntVec2 operator -(IntVec2 a, IntVec2 b)
        {
            return new IntVec2(a.X - b.X, a.Y - b.Y);
        }
        static public IntVec2 operator -(IntVec2 a, int b)
        {
            return new IntVec2(a.X * b, a.Y * b);
        }
        public static bool operator ==(IntVec2 a, IntVec2 b)
        {
            return a.X == b.X && a.Y == b.Y;
        }
        public static bool operator !=(IntVec2 a, IntVec2 b)
        {
            return a.X != b.X || a.Y != b.Y;
        }

        public static implicit operator Vector2(IntVec2 a)
        {
            return new Vector2(a.X, a.Y);
        }
        public static implicit operator IntVec2(Vector2 a)
        {
            return new IntVec2(a);
        }
    }
}
