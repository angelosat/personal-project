using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.Net;
using Start_a_Town_.GameModes;
using Microsoft.Xna.Framework;

namespace Start_a_Town_
{
    class GeneratorPlants : Terraformer
    {
        float Density;// { get; set; }
        List<GameObject.Types> PlantTypes = new List<GameObject.Types>() { 
            GameObject.Types.Tree, 
            GameObject.Types.BerryBush 
        };
        public GeneratorPlants()
        {
            this.ID = Terraformer.Types.Trees;
            this.Name = "Trees";
            this.Density = 0.3f;
        }
        public override void Generate(IMap map)
        {
            var size = Chunk.Size;
            var rand = map.Random;
            foreach (var chunk in map.ActiveChunks.Values)
            {
                for (int i = 0; i < 10; i++)
                {
                    //if (rand.Next(2) > 0)
                    if (rand.Chance(0.5f))
                        continue;
                    var x = rand.Next(0, size);
                    var y = rand.Next(0, size);
                    var z = chunk.HeightMap[x][y];
                    var cell = chunk.GetLocalCell(x, y, z);

                    //var block = cell.Block;// map.GetBlock(x, y, z);
                    if (
                        //block == Block.Soil ||
                        cell.Block == BlockDefOf.Grass)
                    {
                        var plant = rand.Next(2) < 1 ? Plant.CreateTree(PlantDefOf.Tree, map.Biome.Wood,1) : Plant.CreateBush(PlantDefOf.Bush,1,1);
                        int gx = x + (int)chunk.Start.X, gy = y + (int)chunk.Start.Y;
                        plant.Global = new Vector3(gx, gy, z + 1);
                        chunk.Objects.Add(plant);
                    }
                }
            }
        }
        public override void Finally(Chunk chunk)
        {
            return;

            int zSlice = Map.MaxHeight / 2;

            foreach (var plantType in this.PlantTypes)
            {
                //int seedInt = chunk.Map.GetWorld().Seed + "trees".GetHashCode();
                int seedInt = chunk.Map.World.Seed + plantType.ToString().GetHashCode();
                byte[] seed = BitConverter.GetBytes(seedInt);
                for (int lx = 0; lx < Chunk.Size; lx++)
                {
                    for (int ly = 0; ly < Chunk.Size; ly++)
                    {
                        int z = chunk.HeightMap[lx][ly];
                        Cell cell = chunk.GetLocalCell(lx, ly, z);
                        //if (cell.Block.Material != Material.Soil)
                        
                        //if (cell.Block.MaterialType != MaterialType.Soil)
                        if (cell.Block.GetMaterial(cell.BlockData).Type != MaterialType.Soil)
                            continue;

                        int gx = lx + (int)chunk.Start.X, gy = ly + (int)chunk.Start.Y;

                        int oct = 2;
                        double n = 0;
                        for (int o = 0; o < oct; o++)
                            n += Generator.Perlin3D(gx, gy, zSlice, 16 >> o, seed) / Math.Pow(2, o);

                        n += ((this.Density) * 2 - 1);
                        if (n > 0)
                        {
                            Random random = new Random(seedInt + n.GetHashCode());
                            var r = random.NextDouble();
                            if (r < n)
                            {
                                //GameObject tree = GameObject.Create(GameObject.Types.Tree);
                                GameObject tree = GameObject.Create(plantType);

                                var angle = (float)(random.NextDouble() * (Math.PI + Math.PI));
                                var offset = new Vector3((float)Math.Cos(angle), (float)Math.Sin(angle), 0) * .2f;

                                tree.Global = new Vector3(gx, gy, z + 1) + offset;
                                chunk.Objects.Add(tree);
                            }
                        }
                    }
                }
            }
        }
        public void Generate(Chunk chunk)
        {
            var random = chunk.Map.Random;// new Random(seedInt + n.GetHashCode());

            //int zSlice = Map.MaxHeight / 2;
            //int zSlice = (int)(random.NextDouble() * Map.MaxHeight);
            var entities = new LinkedList<GameObject>();
            foreach (var plantType in this.PlantTypes)
            {
                //int seedInt = chunk.Map.World.Seed + plantType.ToString().GetHashCode();
                //byte[] seed = BitConverter.GetBytes(seedInt);
                byte[] seed = new byte[4];
                random.NextBytes(seed);

                for (int lx = 0; lx < Chunk.Size; lx++)
                {
                    for (int ly = 0; ly < Chunk.Size; ly++)
                    {
                        int z = chunk.HeightMap[lx][ly];
                        Cell cell = chunk.GetLocalCell(lx, ly, z);
                        //if (cell.Block.Material != Material.Soil)

                        //if (cell.Block.MaterialType != MaterialType.Soil)
                        if (cell.Block.GetMaterial(cell.BlockData).Type != MaterialType.Soil)
                            continue;

                        int gx = lx + (int)chunk.Start.X, gy = ly + (int)chunk.Start.Y;

                        int oct = 2;
                        double n = 0;
                        for (int o = 0; o < oct; o++)
                            //n += Generator.Perlin3D(gx, gy, zSlice, 16 >> o, seed) / Math.Pow(2, o);
                            n += Generator.Perlin3D(gx, gy, 0, 16 >> o, seed) / Math.Pow(2, o);

                        n += ((this.Density) * 2 - 1);
                        if (n > 0)
                        {
                            var r = random.NextDouble();
                            if (r < n)
                            {
                                //GameObject tree = GameObject.Create(GameObject.Types.Tree);
                                GameObject plant = GameObject.Create(plantType);

                                var angle = (float)(random.NextDouble() * (Math.PI + Math.PI));
                                var offset = new Vector3((float)Math.Cos(angle), (float)Math.Sin(angle), 0) * .2f;

                                plant.Global = new Vector3(gx, gy, z + 1) + offset;
                                //chunk.Objects.Add(tree);
                                //(chunk.Map.Net as Server).InstantiateAndSpawn(tree);
                                entities.AddLast(plant);
                                //chunk.Map.Net.InstantiateAndSpawn(plant);
                            }
                        }
                    }
                }
            }
            PacketEntityInstantiate.Send(Server.Instance, entities);
        }

        public override object Clone()
        {
            return new GeneratorPlants();
        }

        static public void GeneratePlants(IMap map, int x, int y, int z, GameObject.Types plantType)
        {
            var server = map.Net as Server;
            if (server == null)
                return;
            var n = 0;
            var global = new Vector3(x, y, z);
            var radius = 5;
            for (int i = x - radius; i <= x + radius; i++)
                for (int j = y - radius; j <= y + radius; j++)
                {
                    var current = new Vector3(i, j, z);
                    if (!map.IsInBounds(current))
                        continue;
                    var h = map.GetHeightmapValue(current);
                    current = new Vector3(i, j, h);
                    var distance = Vector3.Distance(global, current);
                    if (distance > radius)
                        continue;
                    var cell = map.GetCell(current);
                    if (cell == null)
                        continue;
                    //if (cell.Block.MaterialType != MaterialType.Soil)
                    if (cell.Block.GetMaterial(cell.BlockData).Type != MaterialType.Soil)
                        continue;
                    if (map.GetBlock(current + Vector3.UnitZ) != BlockDefOf.Air)
                        continue;
                    var random = map.Random;// GetWorld().GetRandom();
                    var p = random.NextDouble();
                    var threshold = 1 - Math.Pow(distance / (float)radius, .33f);//.33);
                    //var threshold = 1 - Math.Sqrt(distance / (float)radius);//.33);

                    //threshold.ToConsole();
                    if (p > threshold)
                        continue;

                    var offset = new Vector3(0,0, 1);
                    //offset += new Vector3((float)random.NextDouble() * 2 - 1, (float)random.NextDouble() * 2 - 1, 0);
                    var angle = (float)(random.NextDouble() * (Math.PI + Math.PI));
                    offset += new Vector3((float)Math.Cos(angle), (float)Math.Sin(angle), 0) * .2f;
                    server.Spawn(GameObject.Create(plantType), current + offset);
                    n++;
                }

            n.ToConsole();
            //int seedInt = map.GetWorld().Seed + plantType.ToString().GetHashCode();
            //byte[] seed = BitConverter.GetBytes(seedInt);
            //var g = new Vector3(x, y, zz);
            //int z = map.GetHeightmapValue(g);// chunk.HeightMap[lx][ly];
            //Cell cell = map.GetCell(g);// chunk.GetLocalCell(lx, ly, z);
            ////if (cell.Block.Material != Material.Soil)
            //if (cell.Block.MaterialType != MaterialType.Soil)
            //    continue;

            //int gx = lx + (int)chunk.Start.X, gy = ly + (int)chunk.Start.Y;

            //int oct = 2;
            //double n = 0;
            //for (int o = 0; o < oct; o++)
            //    n += Generator.Perlin3D(gx, gy, zSlice, 16 >> o, seed) / Math.Pow(2, o);

            //n += ((this.Density) * 2 - 1);
            //if (n > 0)
            //{
            //    Random random = new Random(seedInt + n.GetHashCode());
            //    var r = random.NextDouble();
            //    if (r < n)
            //    {
            //        //GameObject tree = GameObject.Create(GameObject.Types.Tree);
            //        GameObject tree = GameObject.Create(plantType);
            //        tree.Global = new Vector3(gx, gy, z + 1);
            //        chunk.Objects.Add(tree);
            //    }
            //}
        }

        internal static void GeneratePlants(IMap map)
        {
            var gen = new GeneratorPlants();
            foreach (var chu in map.ActiveChunks.Values)
                gen.Generate(chu);
        }
    }
}
