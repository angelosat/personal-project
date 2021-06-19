using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.GameModes;

namespace Start_a_Town_.Terraforming.Mutators
{
    class Test : Terraformer
    {
        public Test()
        {
            this.ID = Terraformer.Types.Test;
            this.Name = "Test";
        }
        public override Block.Types Initialize(IWorld w, Cell c, int x, int y, int z, Net.RandomThreaded r)
        {
            double gradient, gradientRock, turbulence, tRock, zNormal,// coal, gold,
               turbpower; //lower? higher turbpower makes floating islands
            int soilLayerThickness = 10;
            float rockDensity = 0.01f;
            byte[] seedArray = w.GetSeedArray();
            //byte[] rockSeed = Generator.Shuffle(seedArray, 5);
            //byte[] coalSeed = Generator.Shuffle(seedArray, 4);
            int octaves = 7;
            Block.Types type = Block.Types.Air;

            zNormal = z / (float)Map.MaxHeight;

            turbulence = 0;
            tRock = 0;

            //turbpower = 1;
            //for (int k = 0; k < octaves; k++)
            //    turbulence += Generator.Perlin3D(i, j, z, 256 >> k, seedArray) * 0.2f;

            // turbpower = 0.2;

            for (int k = 0; k < octaves; k++)
            {
                double intensity = (1 - (k / (float)octaves));
                intensity = intensity * Generator.Perlin3D(x, y, z, 512, seedArray);
                turbulence += Generator.Perlin3D(x, y, z, 256 >> k, seedArray) * intensity;
            }
            //tRock = Generator.Perlin3D(globalX, globalY, globalZ, 16, rockSeed) * 0.4f;

            //coal = 0;
            //coal = Generator.Perlin3D(globalX, globalY, globalZ, 8, coalSeed);//Map.Instance.Seed)
            //coal = (coal + 1) / 2f;

            //gold = 0;
            //for (int k = 0; k < octaves; k++)
            //    gold += Generator.Perlin3D(globalX, globalY, globalZ, 16 >> k, 240);//Map.Instance.Seed)
            //gold /= octaves;
            //gold *= 1 - zNormal;

            gradient = zNormal + zNormal - 1;
            gradient += turbulence;

            //  gradientRock = 2 * ((Math.Max(0, globalZ + soilLayerThickness)) / (float)Map.MaxHeight) - 1 + tRock;// (tRock + 1) / 2f;

            if (z == 0)
                type = Block.Types.Stone;
            else
            {
                if (gradient < Map.GroundDensity)
                {
                    if (gradient > 0)
                    {
                        //if (globalZ <= world.SeaLevel && gradient > Map.GroundDensity - 0.02f)
                        //    type = Block.Types.Sand;
                        //else
                        type = w.DefaultTile;
                    }
                    else
                    {
                        type = Block.Types.Stone;
                        //if (gold > 0.5)
                        //    type = Tile.Types.Coal;
                        //else if (coal > 0.5)
                        //    type = Tile.Types.Coal;
                    }
                }
                //else if (globalZ <= world.SeaLevel)
                //    type = Block.Types.Water;
            }

            //if (gradientRock < rockDensity)
            //{
            //    type = Tile.Types.Cobblestone;
            //    if (coal < 0.3f)
            //        type = Tile.Types.Coal;
            //}
            return type;
            //return type;
        }
        public override object Clone()
        {
            return new Test();
        }
    }
}
