using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.Terraforming.Mutators
{
    class PerlinWormGenerator : Terraformer
    {
        readonly TerraformerProperty CaveCount = new("Cave frequency", 5, 0, 20, 1);
        readonly TerraformerProperty CaveSteps = new("Cave average length", 64, 0, 128, 1); //32

        public override IEnumerable<TerraformerProperty> GetAdjustableParameters()
        {
            yield return this.CaveCount;
            yield return this.CaveSteps;
        }
        public override void Generate(MapBase map)//, Dictionary<IntVec3, double> gradients)
        {
            //return;
            var r = map.World.Random;
            for (int i = 0; i < map.ActiveChunks.Count; i++)
            {
                for (int j = 0; j < this.CaveCount.Value; j++)
                {
                    var x = r.Next(map.GetSizeInChunks() * Chunk.Size);
                    var y = r.Next(map.GetSizeInChunks() * Chunk.Size);
                    var z = r.Next(map.GetHeightmapValue(x, y));
                    var worm = new PerlinWorm(map, new Vector3(x, y, z), (int)this.CaveSteps.Value, map.World.Seed);
                    foreach (var segment in worm.GetBoxes())
                        this.Carve(map, segment);
                }
            }
        }
        public override void Finally(Chunk chunk)
        {
            return;
            var s = chunk.World.Seed;
            var worms = GetWorms(chunk.Map, chunk.MapCoords, s, 3); // chunkradius = 3 means caves can be sourced to each chunk from adjacent chunks
            var size = Chunk.Size;
            var chunkglobal = chunk.MapCoords * size;
            var maxz = MapBase.MaxHeight;
            foreach (var worm in worms)
            {
                var segments = worm.GetBoxes();
                foreach (var segment in segments)
                {
                    var minvec3 = new IntVec3(chunkglobal, 0);
                    var maxvec3 = new IntVec3(chunkglobal + (Chunk.Size - 1) * IntVec2.One, maxz - 1);
                    var chunkbox = new BoundingBox(minvec3, maxvec3);
                    if (chunkbox.Intersects(segment))
                        this.Carve(segment, chunkbox, chunk);
                }
            }
        }
        void Carve(MapBase map, BoundingBox bbox)
        {
            foreach (var current in bbox.GetBoxIntVec3())
            {
                if (current.Z == 0)
                    continue;

                var c = map.GetCell(current);
                if (c is null)
                    continue;
                if (c.Block == BlockDefOf.Air ||
                    c.Block == BlockDefOf.Fluid
                    //||
                    //c.Block == BlockDefOf.Sand
                    )
                    continue;
                c.Block = BlockDefOf.Air;
            }
        }
        void Carve(BoundingBox bbox, BoundingBox chunkbox, Chunk chunk)
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
                            c.Block == BlockDefOf.Fluid
                            //||
                            //c.Block == BlockDefOf.Sand
                            )
                            continue;
                        c.Block = BlockDefOf.Air;
                    }
        }

        List<PerlinWorm> GetWorms(MapBase map, Vector2 chunkCoords, int seed, int chunksRadius)
        {
            var list = new List<PerlinWorm>();
            for (int i = -chunksRadius; i <= chunksRadius; i++)
            for (int j = -chunksRadius; j <= chunksRadius; j++)
            {
                var pos = chunkCoords + new Vector2(i, j);
                list.AddRange(GetWorms(map, pos, seed));
            }
            return list;
        }
        List<PerlinWorm> GetWorms(MapBase map, Vector2 chunkCoords, int seed)
        {
            seed += chunkCoords.GetHashCode();
            var r = new Random(seed);
            var global = new Vector3(chunkCoords, 0) * Chunk.Size;
            var list = new List<PerlinWorm>();
            var wormcount = CaveCount.Value;// 5;// 5;
            var length = (int)CaveSteps.Value;// 32; //16
            for (int i = 0; i < wormcount; i++)
            {
                //var x = (int)(Chunk.Size * r.NextDouble());
                //var y = (int)(Chunk.Size * r.NextDouble());
                //var z = (int)(r.NextDouble() * MapBase.MaxHeight / 2); 
                var x = r.Next(Chunk.Size);
                var y = r.Next(Chunk.Size);
                var z = r.Next(MapBase.MaxHeight / 2);
                /// caves only starting up to the middle of the map's height?? why? 
                /// probably to ensure that no caves will be generated completely in the air?
                /// bad solution eitherway. use the chunk's height map to determine topmost origin Z of the ca
                var origin = global + new Vector3(x, y, z);
                var worm = new PerlinWorm(map, origin, length, seed + origin.GetHashCode());
                list.Add(worm);
            }
            return list;
        }
        class PerlinWorm
        {
            Vector3 CurrentPosition;
            int CurrentStep, MaxLength;
            public float Life { get { return (MaxLength - CurrentStep) / (float)MaxLength; } }
            double pipi = Math.PI * Math.PI;
            float LastRadius;
            Vector3 Origin;
            readonly int Seed;
            MapBase Map;
            public PerlinWorm(MapBase map, Vector3 origin, int maxLength, int seed)
            {
                this.Map = map;
                this.CurrentPosition = origin;
                this.MaxLength = maxLength;
                this.LastRadius = GetRadius(3, 5, this.Life, origin);
                this.Origin = origin;
                this.Seed = seed;
            }

            Vector3 GetNextStep(int seed)
            {
                var current = this.CurrentPosition;
                var nextdir = GetNextDirectionSpherical(CurrentPosition, seed);
                this.CurrentPosition += nextdir;
                this.CurrentStep += 1;
                return current;
            }
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
            float GetRadius(float minRadius, float maxRadius, float life, IntVec3 pos)
            {
                maxRadius = 1;
                minRadius = 1;
                var perc = 1 - Math.Abs(life * 2 - 1);
                var radius = minRadius + perc * (maxRadius - minRadius);

                var z = pos.Z;
                var a = 1 - z / (float)MapBase.MaxHeight;
                radius *= a * a * 3;

                //var gradient = (float)Math.Max(0, -this.Map.GetGradient(pos));
                //var a = gradient;
                //radius *= a * a * 3;

                return radius;
            }
            void CarveWithin(Vector3 global, Chunk chunk)
            {
                var chunkglobal = chunk.MapCoords * Chunk.Size;
                var box = new BoundingBox(new IntVec3(chunkglobal, 0), new IntVec3(chunkglobal + (Chunk.Size - 1) * IntVec2.One, MapBase.MaxHeight - 1));

                var radius = GetRadius(3, 5, this.Life, global);

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
                            var c = chunk.Cells[Chunk.GetCellIndex(local)];
                            if (c.Block == BlockDefOf.Air ||
                                c.Block == BlockDefOf.Fluid ||
                                c.Block == BlockDefOf.Sand)
                                continue;
                            c.Block = BlockDefOf.Air;
                        }
                this.LastRadius = radius;
            }
            
            public IEnumerable<BoundingBox> GetBoxes()
            {
                /// deeper caves should be bigger
                var maxlife = this.MaxLength;
                var current = this.Origin;
                //r *= 2 / (z * z);// ( 1 - (current.Z / MapBase.MaxHeight));
                var r = GetRadius(1, 2, 0, current);

                yield return GetBox(current, r);
                for (int i = 0; i < maxlife; i++)
                {
                    //r *= 2 / (z * z);// ( 1 - (current.Z / MapBase.MaxHeight));
                    current += GetNextDirectionSpherical(current, this.Seed, r);
                    if (!this.Map.Contains(current))
                        continue;
                    r = GetRadius(1, 2, i / (float)maxlife, current);
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
