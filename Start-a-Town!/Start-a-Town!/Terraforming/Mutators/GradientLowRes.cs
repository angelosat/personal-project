using System;
using System.Collections.Generic;
using Start_a_Town_.GameModes;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.Terraforming
{
    class GradientLowRes
    {
        public IWorld World;
        int StartX, StartY;
        static readonly int Hash = "gradient".GetHashCode();
        byte[] Seed;
        int Resolution = 8;
        int Turbulence = 1;
        public GradientLowRes(IWorld world, Chunk chunk, int turb = 1)
        {
            this.World = world;
            this.StartX = (int)chunk.Start.X;
            this.StartY = (int)chunk.Start.Y;
            this.Seed = BitConverter.GetBytes(this.World.GetSeed() + Hash);
            this.Turbulence = turb;
        }
        public GradientLowRes(IWorld world, int startX, int startY, int turb = 1)
        {
            this.World = world;
            this.StartX = startX;
            this.StartY = startY;
            this.Seed = BitConverter.GetBytes(this.World.GetSeed() + Hash);
            this.Turbulence = turb;
        }
        Dictionary<Vector3, double> noiseCache = new Dictionary<Vector3, double>();
        double SampleNoiseAt(int i, int j, int k)
        {
            var x = this.StartX + i;
            var y = this.StartY + j;
            var z = k;
            var xyz = new Vector3(x, y, z);
            var vec = new Vector3(i, j, k);
            double cached;
            if (noiseCache.TryGetValue(vec, out cached))
                return cached;

            var sample = GenerateGradient(x, y, z);
            timessampled++;
            noiseCache[vec] = sample;
            return sample;
        }
        public int timessampled = 0;
        public double GetGradient(int i, int j, int k)
        {
            var res = 8;
            var x1 = res * (i / res);
            var x2 = x1 + res;
            var y1 = res * (j / res);
            var y2 = y1 + res;
            var z1 = res * (k / res);
            var z2 = z1 + res;
            var dx = (i % this.Resolution) / (float)this.Resolution;
            var dy = (j % this.Resolution) / (float)this.Resolution;
            var dz = (k % this.Resolution) / (float)this.Resolution;
           
            var noise = linearInterpolate3d(
                SampleNoiseAt(x1, y1, z1), SampleNoiseAt(x2, y1, z1),
                SampleNoiseAt(x1, y2, z1), SampleNoiseAt(x2, y2, z1),
                SampleNoiseAt(x1, y1, z2), SampleNoiseAt(x2, y1, z2),
                SampleNoiseAt(x1, y2, z2), SampleNoiseAt(x2, y2, z2),
                dx, dy, dz
                );

            return noise;
        }

        private double linearInterpolate3d(double xm_ym_zm, double xp_ym_zm, double xm_yp_zm, double xp_yp_zm,
                                            double xm_ym_zp, double xp_ym_zp, double xm_yp_zp, double xp_yp_zp,
                                            double x, double y, double z)
        {
            return (xm_ym_zm * (1 - x) * (1 - y) * (1 - z)) + (xp_ym_zm * x * (1 - y) * (1 - z)) + (xm_yp_zm * (1 - x) * y * (1 - z)) + (xp_yp_zm * x * y * (1 - z)) +
                    (xm_ym_zp * (1 - x) * (1 - y) * z) + (xp_ym_zp * x * (1 - y) * z) + (xm_yp_zp * (1 - x) * y * z) + (xp_yp_zp * x * y * z);
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
        public double GenerateGradient(int x, int y, int z)
        {
            double grad = 0;
            int octaves = 8;// 6;
            for (int k = 0; k < octaves; k++)
            {
                double kk = Math.Pow(2, octaves - k);
                var f = 1 << k;
                grad += this.Turbulence * Generator.Perlin3D(x, y, z, f, this.Seed) / kk;

            }
            return grad / octaves;
        }
    }
}
