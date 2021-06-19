using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.Net;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.Terraforming.Mutators
{
    class Flowers : Terraformer
    {
        float Density;// { get; set; }
        public Flowers()
        {
            this.ID = Terraformer.Types.Flowers;
            this.Name = "Flowers";
            this.Density = 0.3f;
        }
        public override void Finally(Chunk chunk)
        {
            int zSlice = Map.MaxHeight / 2;
            int varCount = BlockGrass.FlowerOverlays.Count;// Block.Registry[Block.Types.Flowers].Variations.Count;
            byte[][] seed = new byte[varCount][];// BitConverter.GetBytes(chunk.Map.World.Seed);
            int s = chunk.Map.World.Seed + "flowers".GetHashCode();
            var random = new Random(s);
            for (int i = 0; i < varCount; i++)
                seed[i] = BitConverter.GetBytes(s * i);
            var size = Chunk.Size;
            for (int lx = 0; lx < size; lx++)
            {
                for (int ly = 0; ly < size; ly++)
                {
                    Cell cell = chunk.GetLocalCell(lx, ly, chunk.HeightMap[lx][ly]);
                    //if (cell.Block.Type != Block.Types.Grass)
                    if (cell.Block != BlockDefOf.Grass)
                        continue;

                    int gx = lx + (int)chunk.Start.X, gy = ly + (int)chunk.Start.Y;

                    //double[] r = new double[varCount];
                    //for (int i = 0; i < varCount; i++)
                    //{
                    //    double n = Generator.Perlin3D(gx, gy, zSlice, 16, seed[i]);
                    //    //max = Math.Max(max, n);
                    //}

                    int oct = 2;
                    double[] n = new double[varCount];
                    double nmax = 0;
                    int flowermax = 0;
                    for (int i = 0; i < varCount; i++)
                    {
                        n[i] = 0;
                        var seedByte = seed[i];
                        for (int o = 0; o < oct; o++)
                            n[i] += Generator.Perlin3D(gx, gy, zSlice, 16 >> o, seedByte) / Math.Pow(2, o);
                        if (n[i] > nmax)
                        {
                            nmax = n[i]; //slow?
                            flowermax = i;
                        }
                    }

                    //if (nmax > 0)
                    //float factor = (1-this.Density) * 2 - 1;
                    //if (nmax > factor)
                    nmax += ((this.Density) * 2 - 1);
                    if (nmax > 0)
                    {
                        //var random = new Random(s + nmax.GetHashCode());
                        var r = random.NextDouble();
                        if (r < nmax)
                        {
                            //cell.SetBlockType(Block.Types.Flowers);
                            //cell.Variation = (byte)flowermax;
                            cell.BlockData = (byte)(flowermax + 1);
                        }
                    }
                }
            }
        }
        //public void FinallyOld(Chunk chunk)
        //{
        //    int zSlice = Map.MaxHeight / 2;
        //    int varCount = Block.Registry[Block.Types.Flowers].Variations.Count;
        //    byte[][] seed = new byte[varCount][];// BitConverter.GetBytes(chunk.Map.World.Seed);
        //    int s = chunk.Map.World.Seed + "flowers".GetHashCode();
        //    for (int i = 0; i < varCount; i++)
        //        seed[i] = BitConverter.GetBytes(s * i);
            
        //    for (int lx = 0; lx < Chunk.Size; lx++)
        //    {
        //        for (int ly = 0; ly < Chunk.Size; ly++)
        //        {
        //            Cell cell = chunk.GetLocalCell(lx, ly, chunk.HeightMap[lx][ly]);
        //            if (cell.Block.Type != Block.Types.Grass)
        //                continue;

        //            int gx = lx + (int)chunk.Start.X, gy = ly + (int)chunk.Start.Y;
        //            //double[] r = new double[varCount];
        //            //for (int i = 0; i < varCount; i++)
        //            //{
        //            //    double n = Generator.Perlin3D(gx, gy, zSlice, 16, seed[i]);
        //            //    //max = Math.Max(max, n);
        //            //}

        //            int oct = 2;
        //            double[] n = new double[varCount];
        //            double nmax = 0;
        //            int flowermax = 0;
        //            for (int i = 0; i < varCount; i++)
        //            {
        //                n[i] = 0;
        //                for (int o = 0; o < oct; o++)
        //                    n[i] += Generator.Perlin3D(gx, gy, zSlice, 16 >> o, seed[i]) / Math.Pow(2, o);
        //                if (n[i] > nmax)
        //                {
        //                    nmax = n[i];
        //                    flowermax = i;
        //                }
        //            }

        //            //if (nmax > 0)
        //            //float factor = (1-this.Density) * 2 - 1;
        //            //if (nmax > factor)
        //            nmax += ((this.Density)*2 - 1);
        //            if(nmax > 0)
        //            {
        //                var random = new Random(s + nmax.GetHashCode());
        //                var r = random.NextDouble();
        //                if (r < nmax)
        //                {
        //                    //cell.Type = Block.Types.Flowers;
        //                    cell.SetBlockType(Block.Types.Flowers);
        //                    cell.Variation = (byte)flowermax;
        //                }
        //            }
        //        }
        //    }
        //}
        public override object Clone()
        {
            return new Flowers();
        }
    }
}
