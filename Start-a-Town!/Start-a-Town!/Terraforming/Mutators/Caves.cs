using System;
using System.Diagnostics;
using Start_a_Town_.GameModes;

namespace Start_a_Town_.Terraforming.Mutators
{
    /// <summary>
    /// TODO: add a height factor to create bigger caves towards bottom of map
    /// </summary>
    class Caves : Terraformer
    {
        static readonly int Hash1 = "ridged1".GetHashCode();
        static readonly int Hash2 = "ridged2".GetHashCode();

        byte[] RidgedSeed;
        byte[] RidgedSeed2;
        int CaveFrequency = 16;//64
        float Threshold = 0.1f;//0.05f
        public Caves()
        {
            this.ID = Terraformer.Types.Caves;
            this.Name = "Caves";
        }
        static public Stopwatch Watch = new Stopwatch();
        public override void Initialize(IWorld w, Cell c, int x, int y, int z, double g)
        {
            Watch.Start();
            if (z == 0)
                return;
            if (c.Block == BlockDefOf.Air ||
                c.Block == BlockDefOf.Water ||
                c.Block == BlockDefOf.Sand)
                return;

            double ridged = Generator.Perlin3D(x, y, z, this.CaveFrequency, this.RidgedSeed);

            double ridged2 = Generator.Perlin3D(x, y, z, this.CaveFrequency, this.RidgedSeed2);
            //double multi = ridged * ridged2;

            if (ridged > -this.Threshold && ridged < this.Threshold && ridged2 > -this.Threshold && ridged2 < this.Threshold)
                if (z > 0)
                    c.Block = BlockDefOf.Air;
            Watch.Stop();
        }

        private static void CavesVariation(Cell c, int z, double ridged)
        {
            float sealevel = Map.MaxHeight / 2f;
            float distanceFromSeaLevel = (sealevel - z) / sealevel;
            if (ridged * distanceFromSeaLevel < -.2f)
                c.Block = BlockDefOf.Air;
        }
        public override Terraformer SetWorld(IWorld w)
        {
            this.RidgedSeed = BitConverter.GetBytes(w.GetSeed() + Hash1);
            this.RidgedSeed2 = BitConverter.GetBytes(w.GetSeed() + Hash2);
            return this;
        }
        public override object Clone()
        {
            return new Caves();
        }
    }
}
