using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Start_a_Town_
{
    class Terraformers
    {
        //static public Initialize(Chunk chunk, byte[] seedArray)
        //{
        //    chu
        //}
        static public void Flatten(Chunk chunk, byte[] seedArray)
        {
            for (int z = 0; z < chunk.Map.World.SeaLevel; z++)
                for (int x = 0; x < Chunk.Size; x++)
                    for (int y = 0; y < Chunk.Size; y++)
                        chunk[x, y, z].TileType = chunk.Map.World.DefaultTile;
        }

        static public Tile.Types Flat(World world, int globalX, int globalY, int globalZ)
        {
            if (globalZ > world.SeaLevel)
                return Tile.Types.Air;
            return world.DefaultTile;
        }

        static public Tile.Types Normal(World world, int globalX, int globalY, int globalZ)
        {
            double gradient, gradientRock, turbulence, tRock, zNormal, coal, gold,
                   turbpower; //lower? higher turbpower makes floating islands
            int soilLayerThickness = 10;
            float rockDensity = 0.01f;
            byte[] seedArray = world.GetSeedArray();
            byte[] rockSeed = Generator.Shuffle(seedArray, 5);
            byte[] coalSeed = Generator.Shuffle(seedArray, 4);
            int octaves = 2;
            Tile.Types type = Tile.Types.Air;

            zNormal = globalZ / (float)Map.MaxHeight;

            turbulence = 0;
            tRock = 0;

            //turbpower = 1;
            //for (int k = 0; k < octaves; k++)
            //    turbulence += Generator.Perlin3D(i, j, z, 256 >> k, seedArray) * 0.2f;

            turbpower = 0.2;

            for (int k = 0; k < octaves; k++)
            {
                // divide z by 2 to compensate for cells being half-cubes
                //turbulence += Generator.Perlin3D(i, j, z, 32 >> k, seedArray) * turbpower;
                turbulence += Generator.Perlin3D(globalX, globalY, globalZ, 128 >> k, seedArray) * turbpower;
                // tRock += Generator.Perlin3D(i, j, z, 16 >> k, rockSeed)* 0.4f; //Map.Instance.SeedArray
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
                type = Tile.Types.Stone;
            else
            {
                if (gradient < Map.GroundDensity)
                {
                    if (gradient > 0)
                    {
                        //if (z > Map.SeaLevel)
                        //    type = Tile.Types.Soil;
                        //else
                        //    type = Tile.Types.Sand;
                        if (globalZ <=world.SeaLevel && gradient > Map.GroundDensity - 0.02f)
                            type = Tile.Types.Sand;
                        else
                            type = Tile.Types.Soil;
                    }
                    else
                    {
                        type = Tile.Types.Stone;
                        if (gold > 0.5)
                        {
                            type = Tile.Types.Coal;
                            //colors[n] = Color.Lerp(Color.Black, Color.Gold, (float)t/0.3f);
                        }
                        else if (coal > 0.5)
                            type = Tile.Types.Coal;
                    }
                }
                else if (globalZ <= world.SeaLevel)
                {
                    type = Tile.Types.Water;
                    //WaterCells.Push(cell);
                }
            }

            if (gradientRock < rockDensity)
            {
                type = Tile.Types.Cobblestone;
                if (coal < 0.3f)
                    type = Tile.Types.Coal;
            }
            return type;
        }
    }
}
