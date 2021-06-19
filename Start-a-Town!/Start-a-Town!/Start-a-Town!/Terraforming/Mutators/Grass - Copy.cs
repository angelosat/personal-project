using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.Net;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.Terraforming.Mutators
{
    class Grass : Terraformer
    {
        public Grass()
        {
            this.ID = Terraformer.Types.Grass;
            this.Name = "Grass";
            this.Finalize = (RandomThreaded random, World world, Cell cell, int globalX, int globalY, int globalZ) =>
            {
                if (cell.Type != Block.Types.Soil)
                    return;
                int x = globalX, y = globalY, z = globalZ;
                byte[] seedArray = world.GetSeedArray();
                byte[] treeSeed = Generator.Shuffle(seedArray, 15);

                cell.Type = Block.Types.Grass;
                Rectangle[][] rects;
                if (Block.SourceRects.TryGetValue(cell.Type, out rects))
                {
                    //cell.Variation = (byte)world.Random.Next(rects.GetLength(0));
                    //cell.Orientation = (byte)world.Random.Next(rects[cell.Orientation].GetLength(0));
                    cell.Variation = (byte)random.Next(rects.GetLength(0));
                    cell.Orientation = (byte)random.Next(rects[cell.Orientation].GetLength(0));
                }
                double dirt = Generator.Perlin3D(x, y, z, Chunk.Size * 2, Generator.Shuffle(seedArray, 5));
                double dirtDensity = 0.3d;
                double normalDirt = (dirt + 1) / 2.0f;
                double r = random.NextDouble();
                if (normalDirt < dirtDensity)
                {
                    if (r * normalDirt < (dirtDensity - normalDirt) / dirtDensity)
                    {
                        cell.Type = Block.Types.Soil;
                        if (Block.SourceRects.TryGetValue(cell.Type, out rects))
                        {
                            cell.Variation = (byte)random.Next(rects.GetLength(0));
                            cell.Orientation = (byte)random.Next(rects[cell.Orientation].GetLength(0));
                        }
                        return;
                    }
                }

                double[] flowers = new double[] {
                                    Generator.Perlin3D(x, y, z, Chunk.Size, Generator.Shuffle(seedArray, 1)),
                                    Generator.Perlin3D(x, y, z, Chunk.Size, Generator.Shuffle(seedArray, 2)),
                                    Generator.Perlin3D(x, y, z, Chunk.Size, Generator.Shuffle(seedArray, 3)),
                                    Generator.Perlin3D(x, y, z, Chunk.Size, Generator.Shuffle(seedArray, 4))
                                };
                cell.Type = Block.Types.Grass;
                double flowerDensity = 0.3d;
                for (int k = 0; k < flowers.Length; k++)
                {
                    double normal = (flowers[k] + 1) / 2.0f;
                    //   r = Engine.Random.NextDouble();

                    if (normal < flowerDensity)
                    {
                        if (r < flowerDensity)
                        {
                            cell.Type = Block.Types.Flowers;
                            cell.Variation = k;
                        }
                    }
                }
            };
        }
        public override object Clone()
        {
            return new Grass();
        }
    }
}
