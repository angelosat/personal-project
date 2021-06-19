using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Start_a_Town_
{
    public struct IntVec3 : IEquatable<IntVec3>
    {
        public int X, Y, Z;

        public IntVec3(int x, int y, int z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }
        public IntVec3(int xyz) : this(xyz, xyz, xyz)
        {

        }
        public IntVec3(int xy, int z) : this(xy, xy, z)
        {

        }
        public IntVec3(Vector3 vector3) : this((int)Math.Round(vector3.X), (int)Math.Round(vector3.Y), (int)Math.Floor(vector3.Z))
        {

        }
        public IntVec3(float x, float y, float z) : this((int)Math.Round(x), (int)Math.Round(y), (int)Math.Floor(z))
        {

        }
        public bool Equals(IntVec3 other)
        {
            return
                this.X == other.X &&
                this.Y == other.Y &&
                this.Z == other.Z;
        }
        public override bool Equals(object obj)
        {
            return obj is IntVec3 && this.Equals((IntVec3)obj);
        }
        public override int GetHashCode()
        {
            return new Vector3(this.X, this.Y, this.Z).GetHashCode();
        }
        static public IntVec3 Zero = new(0);
        static public IntVec3 One = new(1);
        static public IntVec3 UnitX = new(1, 0, 0);
        static public IntVec3 UnitY = new(0, 1, 0);
        static public IntVec3 UnitZ = new(0, 0, 1);
        //public IntVec3 Above => this + UnitZ;
        //public IntVec3 Below => this - UnitZ;

        //static public IntVec3 operator -(IntVec3 a)
        //{
        //    return new IntVec3(-a.X, -a.Y, -a.Z);
        //}
        static public IntVec3 operator +(IntVec3 a, IntVec3 b)
        {
            return new IntVec3(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        }
        static public IntVec3 operator -(IntVec3 a, IntVec3 b)
        {
            return new IntVec3(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        }
        static public IntVec3 operator -(IntVec3 a, int b)
        {
            return new IntVec3(a.X * b, a.Y * b, a.Z * b);
        }
        public static bool operator ==(IntVec3 a, IntVec3 b)
        {
            return a.X == b.X && a.Y == b.Y && a.Z == b.Z;
        }
        public static bool operator !=(IntVec3 a, IntVec3 b)
        {
            return a.X != b.X || a.Y != b.Y || a.Z != b.Z;
        }

        public static implicit operator Vector3(IntVec3 a)
        {
            return new Vector3(a.X, a.Y, a.Z);
        }
        public static implicit operator IntVec3(Vector3 a)
        {
            return new IntVec3(a);
        }

        public override string ToString()
        {
            return $"{{X:{this.X} Y:{this.Y} Z:{this.Z}}}";
            //return string.Format($"{{X: {this.X} Y: {this.Y} Z: {this.Z}}}");
        }

        // this is a function because it's a leftover from converting vector3 to intvec3 in my code and above() being an extention for vector3's
        public IntVec3 Above() => this + UnitZ;// new IntVec3(this.X, this.Y, this.Z + 1);
        public IntVec3 Below => this - UnitZ;

        public static bool operator ==(IntVec3 a, Vector3 b)
        {
            return a.X == b.X && a.Y == b.Y && a.Z == b.Z;
        }
        public static bool operator !=(IntVec3 a, Vector3 b)
        {
            return a.X != b.X || a.Y != b.Y || a.Z != b.Z;
        }

        public IntVec3 ToGlobal(Chunk chunk)
        {
            return new IntVec3(chunk.Start.X + this.X, chunk.Start.Y + this.Y, this.Z);
        }
        
        public IntVec3 ToLocal()
        {
            float lx, ly;
            lx = this.X % Chunk.Size;
            lx = lx < 0 ? lx + Chunk.Size : lx;
            ly = this.Y % Chunk.Size;
            ly = ly < 0 ? ly + Chunk.Size : ly;
            return new IntVec3(lx, ly, this.Z);
        }

        public IntVec2 GetChunkCoords()
        {
            int chunkX = this.X / Chunk.Size;
            int chunkY = this.Y / Chunk.Size;
            return new IntVec2(chunkX, chunkY);
        }
    }
}
