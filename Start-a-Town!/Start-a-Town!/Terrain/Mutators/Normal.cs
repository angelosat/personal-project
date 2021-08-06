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

        public override Block Initialize(IWorld w, Cell c, int x, int y, int z, Net.RandomThreaded r)
        {
            double gradientSoil, gradientRock, turbulence, tRock, zNormal; //turbpower; //lower? higher turbpower makes floating islands

            zNormal = z / (float)MapBase.MaxHeight - 0.5f;

            turbulence = 0;
            tRock = 0;

            byte[] magnitudeSeed = BitConverter.GetBytes(w.Seed + HashMagnitude);
            byte[] rockSeed = BitConverter.GetBytes(w.Seed + HashRock);

            //turbpower = 0.1;

            int octaves = 8;
            for (int k = 0; k < octaves; k++)
            {
                double kk = Math.Pow(2, k);
                double tsoil = Generator.Perlin3D(x, y, z, 0x100 >> k, magnitudeSeed) / kk;
                tRock += tsoil * 5f; //* 5f is the turbulence value?
                turbulence += tsoil;
            }
            turbulence /= octaves;
            tRock /= octaves;

            gradientSoil = zNormal + turbulence;

            gradientRock = zNormal + tRock + SoilThickness;

            if (gradientRock < this.GroundRatio)
            {
                c.Variation = (byte)Terraformer.GetRandom(rockSeed, x, y, z, 0, BlockDefOf.Cobblestone.Variations.Count);
                return BlockDefOf.Cobblestone;
            }

            if (z == 0)
            {
                c.Block = BlockDefOf.Stone;
                return BlockDefOf.Stone;
            }
            if (gradientSoil <= this.GroundRatio)
            {
                c.Variation = (byte)Terraformer.GetRandom(rockSeed, x, y, z, 0, BlockDefOf.Soil.Variations.Count);
                return w.DefaultBlock;
            }
            // if we didn't add a block yet, it means we have air. so add water if below sealevel
            else
            {
                var seaLevel = MapBase.MaxHeight/2;
                if (z <= seaLevel)
                {
                    if (z < seaLevel)
                        c.BlockData = Blocks.BlockWater.GetData(1);
                    return BlockDefOf.Water;
                }
            }
            return BlockDefOf.Air;
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
                return;
            }
            
            if (gradientSoil <= this.GroundRatio)
            {
                c.Variation = (byte)this.SoilRandomizer.Next(BlockDefOf.Soil.Variations.Count);
                c.Block = BlockDefOf.Soil;
                return;
            }

            var sandThickness = .01f;
            var sandMaxLevel = seaLevel + 1;// 2;

                if (z <= seaLevel)
                    c.Block = BlockDefOf.Water;
                if (z < seaLevel)
                    c.BlockData = Blocks.BlockWater.GetData(1);
                if (z <= sandMaxLevel)
                    if (gradientSoil < this.GroundRatio + sandThickness)
                        c.Block = BlockDefOf.Sand;
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
