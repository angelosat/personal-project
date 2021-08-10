using System;
using System.Collections.Generic;
using Start_a_Town_.GameModes;

namespace Start_a_Town_.Terraforming.Mutators
{
    class Normal : Terraformer
    {
        static readonly int HashMagnitude = "75745".GetHashCode();
        static readonly int HashRock = "rock".GetHashCode();
        static readonly int HashSoil = "soil".GetHashCode();

        float GroundRatio;
        float SoilThickness;
        Random SoilRandomizer = new Random(HashSoil);

        public Normal()
        {
            this.ID = Terraformer.Types.Normal;
            this.Name = "Normal";
            this.SoilThickness = .02f;

            this.GroundRatio = 0f;// .5f;
        }

        public override void Initialize(IWorld w, Cell c, int x, int y, int z, double gradient)
        {
            if (z == 0)
            {
                c.Block = BlockDefOf.Stone;
                return;
            }
            var maxZ = (float)MapBase.MaxHeight;
            float zNormal = z / maxZ - 0.5f;
            double gradientSoil = zNormal + gradient;
            var rockTurbulence = 2;//5
            double gradientRock = zNormal + gradient * rockTurbulence + SoilThickness;
            var seaLevel = maxZ / 2 - 2;

            if (gradientRock < this.GroundRatio)
            {
                c.Block = BlockDefOf.Cobblestone;
                c.Material = MaterialDefOf.Stone;
                return;
            }

            if (gradientSoil <= this.GroundRatio)
            {
                c.Variation = (byte)this.SoilRandomizer.Next(BlockDefOf.Soil.Variations.Count);
                c.Block = BlockDefOf.Soil;
                c.Material = MaterialDefOf.Soil;
                return;
            }

            var sandThickness = .01f;
            var sandMaxLevel = seaLevel + 1;// 2;

            if (z <= seaLevel)
            {
                c.Block = BlockDefOf.Fluid;
                c.Material = MaterialDefOf.Water;
            }
            if (z < seaLevel)
                c.BlockData = Blocks.BlockFluid.GetData(1);
            if (z <= sandMaxLevel)
                if (gradientSoil < this.GroundRatio + sandThickness)
                {
                    c.Block = BlockDefOf.Sand;
                    c.Material = MaterialDefOf.Sand;
                }
            return;
        }

        public override object Clone()
        {
            return new Normal();
        }
        public override List<MutatorProperty> GetAdjustableParameters()
        {
            return new List<MutatorProperty>()
            {
                new MutatorProperty("Ground ratio", this.GroundRatio, 0, 1),
                new MutatorProperty("Soil thickness", this.SoilThickness, 0, 1)
            };
        }
    }
}
