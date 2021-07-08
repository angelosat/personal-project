using System;
using System.Collections.Generic;
using Start_a_Town_.Net;
using Microsoft.Xna.Framework;

namespace Start_a_Town_
{
    class GeneratorPlants : Terraformer
    {
        float Density;
        
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
                    if (rand.Chance(0.5f))
                        continue;
                    var x = rand.Next(0, size);
                    var y = rand.Next(0, size);
                    var z = chunk.HeightMap[x][y];
                    var cell = chunk.GetLocalCell(x, y, z);

                    if (
                        cell.Block == BlockDefOf.Grass)
                    {
                        throw new NotImplementedException();
                        GameObject plant = null;// rand.Next(2) < 1 ? Plant.CreateTree(PlantDefOf.Tree, map.Biome.Wood,1) : Plant.CreateBush(PlantDefOf.Bush,1,1);
                        int gx = x + (int)chunk.Start.X, gy = y + (int)chunk.Start.Y;
                        plant.Global = new Vector3(gx, gy, z + 1);
                        chunk.Objects.Add(plant);
                    }
                }
            }
        }
        public override object Clone()
        {
            return new GeneratorPlants();
        }

    }
}
