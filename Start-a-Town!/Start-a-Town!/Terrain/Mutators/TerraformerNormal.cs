using System;
using System.Collections.Generic;

namespace Start_a_Town_.Terraforming.Mutators
{
    class TerraformerNormal : Terraformer
    {
        static readonly int HashMagnitude = "75745".GetHashCode();
        static readonly int HashRock = "rock".GetHashCode();
        static readonly int HashSoil = "soil".GetHashCode();
        
        readonly Random SoilRandomizer = new(HashSoil);

        readonly TerraformerProperty GroundAirThresholdProp = new("Land/air threshold", 0f, -.3f, .3f, .01f, "0.00");// "##0%");
        readonly TerraformerProperty SoilDepthProp = new("Soil layer depth", .02f, 0, 1, .01f, "##0%"); // .5f;

        public float GroundAirThreshold
        {
            get => this.GroundAirThresholdProp.Value;
            set => this.GroundAirThresholdProp.Value = value;
        }
        public float SoilDepth
        {
            get => this.SoilDepthProp.Value;
            set => this.SoilDepthProp.Value = value;
        }

        public double GetSoilGradient(int z, double gradient)
        {
            float zNormal = z / (float)MapBase.MaxHeight - 0.5f;
            return zNormal + gradient;
        }

        public override void Initialize(WorldBase w, Cell c, int x, int y, int z, double gradient)
        {
            w.GroundAirThreshold = this.GroundAirThreshold;
            if (z == 0)
            {
                c.Block = BlockDefOf.Stone;
                return;
            }
            var maxZ = (float)MapBase.MaxHeight;
            float zNormal = z / maxZ - 0.5f;
            double gradientSoil = zNormal + gradient;
            var rockTurbulence = 2;//5
            double gradientRock = zNormal + gradient * rockTurbulence + this.SoilDepth;

            if (gradientRock < this.GroundAirThreshold)
            {
                c.Block = BlockDefOf.Cobblestone;
                c.Material = MaterialDefOf.Stone;
                return;
            }

            if (gradientSoil <= this.GroundAirThreshold)
            {
                c.Variation = (byte)this.SoilRandomizer.Next(BlockDefOf.Soil.Variations.Count);
                c.Block = BlockDefOf.Soil;
                c.Material = MaterialDefOf.Soil;
                return;
            }

            /// MOVING CODE BELOW TO THE SEA TERRAFORMER

            //var seaLevel = maxZ / 2 - 2;
            //var sandThickness = .01f;
            //var sandMaxLevel = seaLevel + 1;// 2;

            //if (z <= seaLevel)
            //{
            //    c.Block = BlockDefOf.Fluid;
            //    c.Material = MaterialDefOf.Water;
            //}
            //if (z < seaLevel)
            //    c.BlockData = BlockFluid.GetData(1);

            //if (z <= sandMaxLevel)
            //    if (gradientSoil < this.GroundAirThreshold + sandThickness)
            //    {
            //        c.Block = BlockDefOf.Sand;
            //        c.Material = MaterialDefOf.Sand;
            //    }

            return;
        }

        public override IEnumerable<TerraformerProperty> GetAdjustableParameters()
        {
            yield return GroundAirThresholdProp;// = new TerraformerProperty("Ground ratio", this.GroundRatio, 0, 1, .01f, "##0%");
            yield return SoilDepthProp;// new TerraformerProperty("Soil thickness", this.SoilThickness, 0, 1, .01f, "##0%");
        }
    }
}
