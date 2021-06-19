using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_.Terraforming.Mutators
{
    class Normal : Terraformer
    {
        public Normal()
        {
            this.ID = Terraformer.Types.Normal;
            this.Name = "Normal";
            this.InitializeFunc = (World world, Cell cell, int globalX, int globalY, int globalZ) =>
            {
                double gradient, gradientRock, turbulence, tRock, zNormal, coal, gold,
               turbpower; //lower? higher turbpower makes floating islands
                int soilLayerThickness = 10;
                float rockDensity = 0.01f;
                byte[] seedArray = world.GetSeedArray();
                byte[] rockSeed = Generator.Shuffle(seedArray, 5);
                byte[] coalSeed = Generator.Shuffle(seedArray, 4);
                int octaves = 2;
                Block.Types type = Block.Types.Air;

                zNormal = globalZ / (float)Map.MaxHeight;

                turbulence = 0;
                tRock = 0;

                //turbpower = 1;
                //for (int k = 0; k < octaves; k++)
                //    turbulence += Generator.Perlin3D(i, j, z, 256 >> k, seedArray) * 0.2f;

                turbpower = 0.2;

                for (int k = 0; k < octaves; k++)
                {
                    //turbulence += Generator.Perlin3D(globalX, globalY, globalZ, 128 >> k, seedArray) * turbpower;
                    double
                        s1 = Generator.Perlin3D(globalX, globalY, globalZ, 516 >> k, seedArray) * 3,
                        s2 = Generator.Perlin3D(globalX, globalY, globalZ, 32 >> k, Generator.Shuffle(seedArray, 1)) * turbpower;
                    turbulence += s1 * s2;
                }

                tRock = Generator.Perlin3D(globalX, globalY, globalZ, 16, rockSeed) * 0.4f;

                coal = 0;

                coal = Generator.Perlin3D(globalX, globalY, globalZ, 8, coalSeed);//Map.Instance.Seed)
                coal = (coal + 1) / 2f;
                //for (int k = 0; k < octaves; k++)
                //    coal += Generator.Perlin3D(i, j, z, 16 >> k, coalSeed);//Map.Instance.Seed)
                //coal /= octaves;

                gold = 0;
                for (int k = 0; k < octaves; k++)
                    gold += Generator.Perlin3D(globalX, globalY, globalZ, 16 >> k, 240);//Map.Instance.Seed)
                gold /= octaves;
                gold *= 1 - zNormal;


                //gradient = t + t - 1;
                //noise = fbm*2 + gradient < 0 ? -1 : 1;

                ////fbm = Math.Pow(fbm, 0.2);
                //t += Math.Pow(fbm, 0.5);
                ////t = Math.Pow(t, 0.5);//5);
                //gradient = t + t - 1;
                ////gradient = Math.Pow(gradient, 0.2);//5);

                //t += fbm;
                //t = Math.Pow(t, 0.2);//5);
                gradient = zNormal + zNormal - 1;
                gradient += turbulence;

                gradientRock = 2 * ((Math.Max(0, globalZ + soilLayerThickness)) / (float)Map.MaxHeight) - 1 + tRock;// (tRock + 1) / 2f;
                //if ((int)map.HeightMap[i, j] < h && h <= Map.seaLevel)
                //    tile = new Water(cell);
                ////else if (h <= (int)map.HeightMap[i, j])
                if (globalZ == 0)
                    //tile = TileBase.Create(TileBase.Types.Stone);
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
                                type = world.DefaultTile;
                        }
                        else
                        {
                            type = Block.Types.Stone;
                            if (gold > 0.5)
                            {
                                type = Block.Types.Coal;
                            }
                            else if (coal > 0.5)
                                type = Block.Types.Coal;
                        }
                    }
                    //else if (globalZ <= world.SeaLevel)
                    //    type = Block.Types.Water;
                }

                if (gradientRock < rockDensity)
                {
                    type = Block.Types.Cobblestone;
                    if (coal < 0.3f)
                        type = Block.Types.Coal;
                }
                cell.Type = type;
                //return type;
            };
        }
        public override object Clone()
        {
            return new Normal();
        }
    }
}
