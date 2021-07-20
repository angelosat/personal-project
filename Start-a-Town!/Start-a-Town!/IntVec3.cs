﻿using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

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
        public IntVec3(int xyz, int z) : this(xyz, xyz, xyz)
        {

        }
        public IntVec3(IntVec2 xy, int z) : this(xy.X, xy.Y, z)
        {

        }
        public IntVec3(int a) : this(a, a, a)
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
            return obj is IntVec3 vec && this.Equals(vec);
        }
        public override int GetHashCode()
        {
            return new Vector3(this.X, this.Y, this.Z).GetHashCode();
        }
        public static IntVec3 Zero = new(0);
        public static IntVec3 One = new(1);
        public static IntVec3 UnitX = new(1, 0, 0);
        public static IntVec3 UnitY = new(0, 1, 0);
        public static IntVec3 UnitZ = new(0, 0, 1);
        public IntVec2 XY => new IntVec2(this.X, this.Y);
        public static IntVec3 operator +(IntVec3 a, IntVec3 b)
        {
            return new IntVec3(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        }
        public static IntVec3 operator -(IntVec3 a, IntVec3 b)
        {
            return new IntVec3(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        }
        public static IntVec3 operator -(IntVec3 a)
        {
            return new IntVec3(-a.X, -a.Y, -a.Z);
        }
        public static IntVec3 operator -(IntVec3 a, int b)
        {
            return new IntVec3(a.X * b, a.Y * b, a.Z * b);
        }
        public static IntVec3 operator *(IntVec3 a, int k)
        {
            return new IntVec3(a.X * k, a.Y * k, a.Z * k);
        }
        public static IntVec3 operator *(IntVec3 a, IntVec3 b)
        {
            return new IntVec3(a.X * b.X, a.Y * b.Y, a.Z * b.Z);
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
        }

        public IntVec3 Above => this + UnitZ;
        public IntVec3 Below => this - UnitZ;
        public IntVec3 North => this + UnitY;
        public IntVec3 South => this - UnitY;
        public IntVec3 East => this + UnitX;
        public IntVec3 West => this - UnitX;

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

        public IEnumerable<IntVec3> GetNeighbors()
        {
            yield return this + UnitX;
            yield return this - UnitX;
            yield return this + UnitY;
            yield return this - UnitY;
            yield return this + UnitZ;
            yield return this - UnitZ;
        }

        public float LengthSquared()
        {
            return this.X * this.X + this.Y * this.Y + this.Z * this.Z;
        }
    }
}
