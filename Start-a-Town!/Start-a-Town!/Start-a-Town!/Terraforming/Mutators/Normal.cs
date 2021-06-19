using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.GameModes;

namespace Start_a_Town_.Terraforming.Mutators
{
    class Normal : Terraformer
    {
        static readonly int HashMagnitude = "75745".GetHashCode();
        static readonly int HashRock = "rock".GetHashCode();
        static readonly int HashSoil = "soil".GetHashCode();

        float GroundRatio;// { get; set; }
        float SoilThickness;// { get; set; }
        Random SoilRandomizer = new Random(HashSoil);
        Random RockRandomizer = new Random(HashRock);

        public Normal()
        {
            this.ID = Terraformer.Types.Normal;
            this.Name = "Normal";
            this.SoilThickness = .025f;// 0.01f;// 5f;// 0.1f;
            this.GroundRatio = 0f;// .5f;
        }

        public override Block.Types Initialize(IWorld w, Cell c, int x, int y, int z, Net.RandomThreaded r)
        {
            double gradientSoil, gradientRock, turbulence, tRock, zNormal,
              turbpower; //lower? higher turbpower makes floating islands
            //    int soilLayerThickness = 10;
            //float rockDensity = 0.01f;
            byte[] seedArray = w.GetSeedArray();
            //byte[] rockSeed = Generator.Shuffle(seedArray, 5);
            //byte[] coalSeed = Generator.Shuffle(seedArray, 4);
            

            zNormal = z / (float)Map.MaxHeight - 0.5f;

            turbulence = 0;
            tRock = 0;

            //turbpower = 1;
            //for (int k = 0; k < octaves; k++)
            //    turbulence += Generator.Perlin3D(i, j, z, 256 >> k, seedArray) * 0.2f;
            byte[] magnitudeSeed = BitConverter.GetBytes(w.Seed + HashMagnitude);
            byte[] rockSeed = BitConverter.GetBytes(w.Seed + HashRock);

            turbpower = 0.1;

            int octaves = 8;
            for (int k = 0; k < octaves; k++)
            {
                double kk = Math.Pow(2, k);
                double tsoil = Generator.Perlin3D(x, y, z, 0x100 >> k, magnitudeSeed) / kk;
                tRock += tsoil * 5f; //* 5f
                turbulence += tsoil;
            }
            turbulence /= octaves;
            tRock /= octaves;

            gradientSoil = zNormal + turbulence;
            //tRock = Generator.Perlin3D(x, y, z, 16, rockSeed) * 0.4f;

            gradientRock = zNormal + tRock + SoilThickness;// * (tRock + 1) / 2f;

            if (gradientRock < this.GroundRatio)//(this.GroundRatio * (1 - SoilThickness)))// rockDensity)// this.SoilThickness * GroundLevel)
            {
                c.Variation = (byte)Terraformer.GetRandom(rockSeed, x, y, z, 0, Block.Registry[Block.Types.Cobblestone].Variations.Count);
                return Block.Types.Cobblestone;
            }

            if (z == 0)
            {
                c.Block = Block.Stone;
                return Block.Types.Stone;
            }
            if (gradientSoil <= this.GroundRatio)
            {
                c.Variation = (byte)Terraformer.GetRandom(rockSeed, x, y, z, 0, Block.Registry[Block.Types.Soil].Variations.Count);
                return w.DefaultTile;
            }
            // if we didn't add a block yet, it means we have air. so add water if below sealevel
            else
            {
                var seaLevel = Map.MaxHeight/2;
                if (z <= seaLevel)
                {
                    if (z < seaLevel)
                        c.BlockData = Blocks.BlockWater.GetData(1);
                    return Block.Types.Water;
                }
            }
            return Block.Types.Air;
        }

        public override void Initialize(IWorld w, Cell c, int x, int y, int z, double gradient)
        {
            if (z == 0)
            {
                //c.Type = Block.Types.Stone;
                c.Block = Block.Stone;
                //c.SetBlockType(Block.Types.Stone);
                return;
            }

            float zNormal = z / (float)Map.MaxHeight - 0.5f;
            double gradientSoil = zNormal + gradient;
            double gradientRock = zNormal + gradient * 5 + SoilThickness;
            var seaLevel = Map.MaxHeight / 2 - 2;
            //if (gradientRock < this.GroundRatio)


            //if (gradientSoil < -.3f) // for deep open space (hell?)
            //{
            //    c.Block = Block.Air;
            //    return;
            //}


            if (gradientSoil < this.GroundRatio - this.SoilThickness)
            {
                Random random = this.RockRandomizer;// new Random(gradient.GetHashCode() + HashRock);
                //c.Variation = (byte)random.Next(Block.Cobblestone.Variations.Count);
                //c.Block = Block.Cobblestone;
                c.Block = Block.Mineral;
                return;
            }

            
            if (gradientSoil <= this.GroundRatio)
            {
                //var sandThickness = .01f;
                //if (gradientSoil > this.GroundRatio - sandThickness)
                //{
                //    c.Block = Block.Sand;
                //    return;
                //}
                Random random = this.SoilRandomizer;// new Random(gradient.GetHashCode() + HashSoil);
                //c.Variation = (byte)random.Next(Block.Registry[Block.Types.Soil].Variations.Count);
                c.Variation = (byte)random.Next(Block.Soil.Variations.Count);

                //c.Type = w.DefaultTile;
                c.Block = Block.Soil; // TODO: store the block class in the world instead of just the type, to reduce lookup
                //c.SetBlockType(w.DefaultTile);// store the block class in the world instead of just the type, to reduce lookup
                return;
            }

            
            //else
            //{
            var sandThickness = .01f;
            var sandMaxLevel = seaLevel + 2;

                if (z <= seaLevel)
                    c.Block = Block.Water;
                if (z < seaLevel)
                    c.BlockData = Blocks.BlockWater.GetData(1);
                if (z <= sandMaxLevel)
                    if (gradientSoil < this.GroundRatio + sandThickness)
                        c.Block = Block.Sand;
                return;
            //}
            c.Block = Block.Air;

        }

        //internal override void Finally(Chunk newChunk, Dictionary<Microsoft.Xna.Framework.Vector3, double> gradients)
        //{
        //    var seaLevel = Map.MaxHeight / 2;
        //    float zNormal;// = z / (float)Map.MaxHeight - 0.5f;

        //    foreach (var c in newChunk.CellGrid2)
        //    {

        //        var z = c.Z;
        //        var gradient = gradients[c.LocalCoords];
        //        zNormal = z / (float)Map.MaxHeight - 0.5f;

        //        double gradientSoil = zNormal + gradient;


        //        var sandThickness = .01f;

        //        if (c.Block == Block.Air)
        //        {
        //            if (z <= seaLevel)
        //            {
        //                c.Block = Block.Water;

        //                if (z < seaLevel)
        //                    c.BlockData = Blocks.BlockWater.GetData(1);
        //            }
        //        }
        //        else
        //        {
        //            if (z <= seaLevel + 2)
        //                if (gradientSoil < this.GroundRatio + sandThickness)
        //                    c.Block = Block.Sand;
        //        }

        //    }
        //}

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
