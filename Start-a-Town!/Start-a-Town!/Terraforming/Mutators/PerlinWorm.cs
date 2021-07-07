using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.Terraforming.Mutators
{
    class PerlinWormGenerator : Terraformer
    {
        public PerlinWormGenerator()
        {
            this.ID = Terraformer.Types.PerlinWorms;
            this.Name = "PerlinWorms";
        }
        public override void Finally(Chunk chunk)
        {
            var s = chunk.World.Seed;
            var worms = GetWorms(chunk.MapCoords, s, 3);
            var size = Chunk.Size;
            var chunkglobal = chunk.MapCoords * size;
            var maxz = Map.MaxHeight;
            foreach (var worm in worms)
            {
                var segments = worm.GetBoxes();
                foreach (var segment in segments)
                {
                    var minvec3 = new Vector3(chunkglobal, 0);
                    var maxvec3 = new Vector3(chunkglobal + (Chunk.Size - 1) * Vector2.One, maxz - 1);
                    var chunkbox = new BoundingBox(minvec3, maxvec3);
                    if (chunkbox.Intersects(segment))
                        this.Carve(segment, chunkbox, chunk);
                }
                
            }
        }

        public void Carve(BoundingBox bbox, BoundingBox chunkbox, Chunk chunk)
        {
            int x = (int)bbox.Min.X;
            int y = (int)bbox.Min.Y;
            int z = (int)bbox.Min.Z;
            int xx = (int)bbox.Max.X;
            int yy = (int)bbox.Max.Y;
            int zz = (int)bbox.Max.Z;

            for (int i = x; i <= xx; i++)
                for (int j = y; j <= yy; j++)
                    for (int k = z; k <= zz; k++)
                    {
                        if (k == 0)
                            continue;

                        var current = new Vector3(i, j, k);
                        var contains = chunkbox.Contains(current);
                        if (contains == ContainmentType.Disjoint)
                            continue;
                        var local = current.ToLocal();
                        var c = chunk.GetCellLocal(local);
                        if (c.Block == BlockDefOf.Air ||
                            c.Block == BlockDefOf.Water ||
                            c.Block == BlockDefOf.Sand)
                            continue;
                        c.Block = BlockDefOf.Air;
                    }
        }


        public override object Clone()
        {
            return new PerlinWormGenerator();
        }

        static List<PerlinWorm> GetWorms(Vector2 chunkCoords, int seed, int radius)
        {
            var list = new List<PerlinWorm>();
            for (int i = -radius; i <= radius; i++)
            for (int j = -radius; j <= radius; j++)
            {
                var pos = chunkCoords + new Vector2(i, j);
                list.AddRange(GetWorms(pos, seed));
            }
            return list;
        }
        static List<PerlinWorm> GetWorms(Vector2 chunkCoords, int seed)
        {
            seed += chunkCoords.GetHashCode();
            var r = new Random(seed);
            var global = new Vector3(chunkCoords, 0) * Chunk.Size;
            var list = new List<PerlinWorm>();
            var wormcount = 5;// 5;
            var length = 32; //16
            for (int i = 0; i < wormcount; i++)
            {
                var x = (int)(Chunk.Size * r.NextDouble());
                var y = (int)(Chunk.Size * r.NextDouble());
                var z = (int)((Map.MaxHeight / 2) * r.NextDouble());
                var pos = global + new Vector3(x, y, z);
                var worm = new PerlinWorm(pos, length, seed + pos.GetHashCode());
                list.Add(worm);
            }
            return list;
        }
        class PerlinWorm
        {
            Vector3 CurrentPosition;
            int CurrentStep, MaxLength;
            public float Life { get { return (MaxLength - CurrentStep) / (float)MaxLength; } }

            float LastRadius;
            Vector3 Origin;
            readonly int Seed;
            public PerlinWorm(Vector3 origin, int maxLength, int seed)
            {
                this.CurrentPosition = origin;
                this.MaxLength = maxLength;
                this.LastRadius = GetRadius(3, 5, this.Life);
                this.Origin = origin;
                this.Seed = seed;
            }
            public Vector3 GetNextStep(int seed)
            {
                var current = this.CurrentPosition;
                var nextdir = GetNextDirectionSpherical(CurrentPosition, seed);
                this.CurrentPosition += nextdir;
                this.CurrentStep += 1;
                return current;
            }
            double pipi = Math.PI * Math.PI;
            Vector3 GetNextDirectionSpherical(Vector3 global, int seed, float radius)
            {
                return this.GetNextDirectionSpherical((int)global.X, (int)global.Y, (int)global.Z, seed, radius);
            }
            Vector3 GetNextDirectionSpherical(Vector3 global, int seed)
            {
                return this.GetNextDirectionSpherical((int)global.X, (int)global.Y, (int)global.Z, seed, this.LastRadius);
            }
            
            Vector3 GetNextDirectionSpherical(int x, int y, int z, int seed, float radius)
            {
                var f = 16;
                
                var s = BitConverter.GetBytes(seed);
                var s2 = BitConverter.GetBytes(seed * seed);

                var rtheta = Generator.Perlin3D(x, y, z, f, s);
                var rphi = Generator.Perlin3D(x, y, z, f, s2);
                
                var atheta = (Math.PI / 2) * (1 + rtheta);
                var aphi = pipi * rphi;

                var rx = (Math.Sin(atheta) * Math.Cos(aphi));
                var ry = (Math.Sin(atheta) * Math.Sin(aphi));
                var rz = Math.Cos(atheta);

                var dir = new Vector3((float)rx, (float)ry, (float)rz);
               
                dir *= radius;
                return dir;
            }
            public float GetRadius(float minRadius, float maxRadius, float life)
            {
                maxRadius = 1;
                minRadius = 1;
                var perc = 1 - Math.Abs(life * 2 - 1);
                var radius = minRadius + perc * (maxRadius - minRadius);
                return radius;
            }
            public void CarveWithin(Vector3 global, Chunk chunk)
            {
                var chunkglobal = chunk.MapCoords * Chunk.Size;
                var box = new BoundingBox(new Vector3(chunkglobal, 0), new Vector3(chunkglobal + (Chunk.Size - 1) * Vector2.One, Map.MaxHeight - 1));

                var radius = GetRadius(3, 5, this.Life);

                int x = (int)Math.Round(global.X - radius);
                int y = (int)Math.Round(global.Y - radius);
                int z = (int)Math.Round(global.Z - radius);
                int xx = (int)Math.Round(global.X + radius);
                int yy = (int)Math.Round(global.Y + radius);
                int zz = (int)Math.Round(global.Z + radius);

                for (int i = x; i <= xx; i++)
                    for (int j = y; j <= yy; j++)
                        for (int k = z; k <= zz; k++)
                        {
                            if (k == 0)
                                continue;

                            var current = new Vector3(i, j, k);
                            var contains = box.Contains(current);
                            if (contains == ContainmentType.Disjoint)
                                continue;
                            var local = current.ToLocal();
                            var c = chunk.CellGrid2[Chunk.GetCellIndex(local)];
                            if (c.Block == BlockDefOf.Air ||
                                c.Block == BlockDefOf.Water ||
                                c.Block == BlockDefOf.Sand)
                                continue;
                            c.Block = BlockDefOf.Air;
                        }
                this.LastRadius = radius;
            }
            
            public IEnumerable<BoundingBox> GetBoxes()
            {
                var r = GetRadius(1, 2, 0);
                var maxlife = this.MaxLength;
                var current = this.Origin;
                yield return GetBox(current, r);
                for (int i = 0; i < maxlife; i++)
                {
                    r = GetRadius(1, 2, i / (float)maxlife);
                    current += GetNextDirectionSpherical(current, this.Seed, r);
                    var box = GetBox(current, r);
                    yield return box;
                }
            }
            BoundingBox GetBox(Vector3 global, float radius)
            {
                int x = (int)Math.Round(global.X - radius);
                int y = (int)Math.Round(global.Y - radius);
                int z = (int)Math.Round(global.Z - radius);
                int xx = (int)Math.Round(global.X + radius);
                int yy = (int)Math.Round(global.Y + radius);
                int zz = (int)Math.Round(global.Z + radius);
                return new BoundingBox(new Vector3(x, y, z), new Vector3(xx, yy, zz));
            }
        }
    }
}
