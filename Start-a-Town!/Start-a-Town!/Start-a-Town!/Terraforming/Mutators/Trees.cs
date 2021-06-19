using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.Net;
using Start_a_Town_.Components.Materials;
using Start_a_Town_.GameModes;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.Terraforming.Mutators
{
    class Trees : Terraformer
    {
        float Density { get; set; }
        List<GameObject.Types> PlantTypes = new List<GameObject.Types>() { 
            GameObject.Types.Tree, 
            GameObject.Types.BerryBush 
        };
        public Trees()
        {
            this.ID = Terraformer.Types.Trees;
            this.Name = "Trees";
            this.Density = 0.3f;
        }
        public override void Finally(Chunk chunk)
        {
            int zSlice = Map.MaxHeight / 2;

            foreach (var plantType in this.PlantTypes)
            {
                //int seedInt = chunk.Map.GetWorld().Seed + "trees".GetHashCode();
                int seedInt = chunk.Map.GetWorld().Seed + plantType.ToString().GetHashCode();
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
        public override object Clone()
        {
            return new Trees();
        }

        static public void GeneratePlants(IMap map, int x, int y, int z, GameObject.Types plantType)
        {
            var server = map.GetNetwork() as Net.Server;
            if (server == null)
                return;
            var n = 0;
            var global = new Vector3(x, y, z);
            var radius = 5;
            for (int i = x - radius; i <= x + radius; i++)
                for (int j = y - radius; j <= y + radius; j++)
                {
                    var current = new Vector3(i, j, z);
                    if (!map.PositionExists(current))
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
                    if (map.GetBlock(current + Vector3.UnitZ) != Block.Air)
                        continue;
                    var random = map.GetWorld().GetRandom();
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
    }
}
