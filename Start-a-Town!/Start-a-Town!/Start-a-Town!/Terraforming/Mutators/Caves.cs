using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            if (c.Block == Block.Air ||
                c.Block == Block.Water ||
                c.Block == Block.Sand)
                return;


            //byte[] ridgedSeed = BitConverter.GetBytes(w.Seed + Hash1);
            //byte[] ridgedSeed2 = BitConverter.GetBytes(w.Seed + Hash2);
            //double ridged = Generator.Perlin3D(x, y, z, 64, ridgedSeed);
            //double ridged2 = Generator.Perlin3D(x, y, z, 64, ridgedSeed2);

            double ridged = Generator.Perlin3D(x, y, z, this.CaveFrequency, this.RidgedSeed);


            //CavesVariation(c, z, ridged);
            //return;

            double ridged2 = Generator.Perlin3D(x, y, z, this.CaveFrequency, this.RidgedSeed2);
            double multi = ridged * ridged2;

            if (ridged > -this.Threshold && ridged < this.Threshold && ridged2 > -this.Threshold && ridged2 < this.Threshold)
                if (z > 0)
                    //c.Type = Block.Types.Air;
                    c.Block = Block.Air;
                    //c.SetBlockType(Block.Types.Air);
            Watch.Stop();
        }

        private static void CavesVariation(Cell c, int z, double ridged)
        {
            float sealevel = Map.MaxHeight / 2f;
            float distanceFromSeaLevel = (sealevel - z) / sealevel;
            if (ridged * distanceFromSeaLevel < -.2f)
                c.Block = Block.Air;
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
