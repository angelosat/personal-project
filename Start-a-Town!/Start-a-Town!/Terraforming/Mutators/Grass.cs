using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.Net;
using Microsoft.Xna.Framework;
using Start_a_Town_.GameModes;

namespace Start_a_Town_.Terraforming.Mutators
{
    class Grass : Terraformer
    {
        Random Randomizer;

        public Grass()
        {
            this.ID = Terraformer.Types.Grass;
            this.Name = "Grass";
            this.Finalize = (RandomThreaded random, IWorld w, Cell c, int x, int y, int z) =>
            {

            };
        }

        public override Terraformer SetWorld(IWorld w)
        {
            this.Randomizer = new Random(w.Seed + "Grass".GetHashCode());
            return this;
        }

        public override void Finally(Chunk chunk)
        {
            int varCount = Block.Registry[Block.Types.Grass].Variations.Count;
            //byte[] seed = BitConverter.GetBytes(chunk.Map.World.Seed);
            //Random varRandomizer = this.Randomizer;// new Random(chunk.Map.World.Seed + "Grass".GetHashCode());
            var count = Chunk.Size;
            for (int lx = 0; lx < count; lx++)
            {
                for (int ly = 0; ly < count; ly++)
                {
                    int z = chunk.HeightMap[lx][ly];
                    var c = chunk.GetLocalCell(lx, ly, z);
                    //if (c.Block.Type != Block.Types.Soil)
                    if (c.Block != BlockDefOf.Soil)
                        continue;
                    //int gx = lx + chunk.X, gy = ly + chunk.Y;

                    //int variation = Terraformer.GetRandom(seed, gx, gy, z, 0, varCount);
                    var variation = (byte)this.Randomizer.Next(varCount);

                    //c.Type = Block.Types.Grass;
                    //c.SetBlockType(Block.Types.Grass);
                    c.Block = BlockDefOf.Grass;
                    c.Variation = variation;
                }
            }
        }

        //public override Block.Types Initialize(World w, Cell c, int x, int y, int z)
        //{
        //        if (cell.Type != Block.Types.Soil)
        //            return;
        //        int x = globalX, y = globalY, z = globalZ;
        //        byte[] seedArray = world.GetSeedArray();
        //        byte[] treeSeed = Generator.Shuffle(seedArray, 15);

        //        cell.Type = Block.Types.Grass;
        //        Rectangle[][] rects;
        //        if (Block.SourceRects.TryGetValue(cell.Type, out rects))
        //        {
        //            //cell.Variation = (byte)world.Random.Next(rects.GetLength(0));
        //            //cell.Orientation = (byte)world.Random.Next(rects[cell.Orientation].GetLength(0));
        //            cell.Variation = (byte)random.Next(rects.GetLength(0));
        //            cell.Orientation = (byte)random.Next(rects[cell.Orientation].GetLength(0));
        //        }
        //        double dirt = Generator.Perlin3D(x, y, z, Chunk.Size * 2, Generator.Shuffle(seedArray, 5));
        //        double dirtDensity = 0.3d;
        //        double normalDirt = (dirt + 1) / 2.0f;
        //        double r = random.NextDouble();
        //        if (normalDirt < dirtDensity)
        //        {
        //            if (r * normalDirt < (dirtDensity - normalDirt) / dirtDensity)
        //            {
        //                cell.Type = Block.Types.Soil;
        //                if (Block.SourceRects.TryGetValue(cell.Type, out rects))
        //                {
        //                    cell.Variation = (byte)random.Next(rects.GetLength(0));
        //                    cell.Orientation = (byte)random.Next(rects[cell.Orientation].GetLength(0));
        //                }
        //                return;
        //            }
        //        }

        //        double[] flowers = new double[] {
        //                            Generator.Perlin3D(x, y, z, Chunk.Size, Generator.Shuffle(seedArray, 1)),
        //                            Generator.Perlin3D(x, y, z, Chunk.Size, Generator.Shuffle(seedArray, 2)),
        //                            Generator.Perlin3D(x, y, z, Chunk.Size, Generator.Shuffle(seedArray, 3)),
        //                            Generator.Perlin3D(x, y, z, Chunk.Size, Generator.Shuffle(seedArray, 4))
        //                        };
        //        cell.Type = Block.Types.Grass;
        //        double flowerDensity = 0.3d;
        //        for (int k = 0; k < flowers.Length; k++)
        //        {
        //            double normal = (flowers[k] + 1) / 2.0f;
        //            //   r = Engine.Random.NextDouble();

        //            if (normal < flowerDensity)
        //            {
        //                if (r < flowerDensity)
        //                {
        //                    cell.Type = Block.Types.Flowers;
        //                    cell.Variation = k;
        //                }
        //            }
        //        }
        //}

        public override object Clone()
        {
            return new Grass();
        }
    }
}
