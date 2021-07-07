using System;
using Start_a_Town_.GameModes;

namespace Start_a_Town_.Terraforming
{
    class Gradient
    {
        public IWorld World;
        static readonly int Hash = "gradient".GetHashCode();
        byte[] Seed;
        public Gradient(IWorld world)
        {
            this.World = world;
            this.Seed = BitConverter.GetBytes(this.World.GetSeed() + Hash);
        }
        public double GetGradientSlow(int x, int y, int z)
        {
            double grad = 0;
            int octaves = 8;
            for (int k = 0; k < octaves; k++)
            {
                double kk = Math.Pow(2, k);
                grad += Generator.Perlin3D(x, y, z, 0x100 >> k, this.Seed) / kk;
            }
            return grad / octaves;
        }
        public double GetGradient(int x, int y, int z)
        {
            double grad = 0;
            int octaves = 8;// 6;
            for (int k = 0; k < octaves; k++)
            {
                double kk = Math.Pow(2, octaves - k);
                var f = 1 << k;
                grad += Generator.Perlin3D(x, y, z, f, this.Seed) / kk;
               
            }
            return grad / octaves;
        }
    }
}
