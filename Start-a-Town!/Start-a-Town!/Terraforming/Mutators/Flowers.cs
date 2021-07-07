using System;

namespace Start_a_Town_.Terraforming.Mutators
{
    class Flowers : Terraformer
    {
        float Density;
        public Flowers()
        {
            this.ID = Terraformer.Types.Flowers;
            this.Name = "Flowers";
            this.Density = 0.3f;
        }
        public override void Finally(Chunk chunk)
        {
            int zSlice = Map.MaxHeight / 2;
            int varCount = BlockGrass.FlowerOverlays.Count;
            byte[][] seed = new byte[varCount][];
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
                    if (cell.Block != BlockDefOf.Grass)
                        continue;

                    int gx = lx + (int)chunk.Start.X, gy = ly + (int)chunk.Start.Y;

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

                    nmax += ((this.Density) * 2 - 1);
                    if (nmax > 0)
                    {
                        var r = random.NextDouble();
                        if (r < nmax)
                        {
                            cell.BlockData = (byte)(flowermax + 1);
                        }
                    }
                }
            }
        }
        
        public override object Clone()
        {
            return new Flowers();
        }
    }
}
