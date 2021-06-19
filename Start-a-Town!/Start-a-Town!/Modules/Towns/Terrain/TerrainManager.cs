using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.Towns;
using Start_a_Town_.GameModes.StaticMaps;

namespace Start_a_Town_
{
    public class TerrainManager : TownComponent
    {
        public TerrainManager(Town town) : base(town)
        {
        }
        bool DensityValidated = false;
        float CurrentDensity;
        int TotalFertileCells;
        int TotalFertileCellsTemp;
        public override string Name => "Terrain";
        int t = 0;
        int CycleIndex;

        Action<IMap, IntVec3>[] SpawnActions;
        //=  
        //{ 
        //    SpawnEntity,
        //    SpawnBlock
        //};
        PlantProperties[] ValidPlants;
       
        public override void Tick()
        {
            if (this.Map.Net is Net.Client)
                return;
            //if (t < Engine.TicksPerSecond * 5)
            //{
            //    t++;
            //    return;
            //}
            //t = 0;
           
            this.GeneratePlant();
        }
        public void GeneratePlant()
        {
            if(!this.DensityValidated)
            {
                this.TotalFertileCells = this.GetTotalFertileCells();
                this.CurrentDensity = this.GetCurrentPlantDensity(this.TotalFertileCells);
                this.DensityValidated = true;
            }
            var map = this.Map as StaticMap;
            var rand = map.Random;
            var num = this.Map.ActiveChunks.Count;
            for (int i = 0; i < num; i++)
            {
                if(this.CycleIndex >= map.Volume)
                {
                    this.CurrentDensity = this.GetCurrentPlantDensity(this.TotalFertileCellsTemp);
                    this.TotalFertileCellsTemp = 0;
                    this.CycleIndex = 0;
                }
                var global = map.GetNextRandomCell();
                var x = global.X;
                var y = global.Y;
                var z = global.Z;
                var cell = map.GetCell(x, y, z);
                var fertility = cell.Fertility;
                if (fertility > 0 )
                {
                    this.TotalFertileCellsTemp++;
                    if (!this.IsSaturated() && rand.Chance(this.Map.PlantDensityTarget) && this.CanGrowOn(global))
                    {
                        var action = (SpawnActions ??= initSpawnActions()).SelectRandom(rand);
                        action(map, new IntVec3(x, y, z));


                        //var plant = rand.Next(2) < 1 ? ItemFactory.CreateFrom(PlantDefOf.Tree, this.Map.Biome.Wood) : Plant.CreateBush(PlantDefOf.BerryBush);
                        //map.Net.Spawn(plant, new Vector3(x, y, z + 1));
                    }
                }
                this.CycleIndex++;
            }

            Action<IMap, IntVec3>[] initSpawnActions()
            {
                return new Action<IMap, IntVec3>[] { SpawnEntity, SpawnBlock };
            }
        }

        private void SpawnEntity(IMap map, IntVec3 global)
        {
            var allPlants = this.ValidPlants ??= this.GetValidPlants();
            var randomPlant = allPlants.SelectRandom(map.Random);
            //var rand = map.Random;
            //var plant = rand.Next(2) < 1 ? ItemFactory.CreateFrom(PlantDefOf.Tree, map.Biome.Wood) : Plant.CreateBush(PlantDefOf.Bush);
            var plant = randomPlant.CreatePlant();
            plant.SyncInstantiate(map.Net);
            map.SyncSpawn(plant, global.Above(), Vector3.Zero);
        }
        private void SpawnBlock(IMap map, IntVec3 global)
        {
            BlockGrass.GrowRandomFlower(map, global);
        }
        PlantProperties[] GetValidPlants()
        {
            return Def.GetDefs<PlantProperties>().ToArray();
        }
        bool CanGrowOn(Vector3 global)
        {
            return !this.Map.GetObjects(global.Above()).Any() && this.Map.GetBlock(global.Above()) == BlockDefOf.Air;

            return !this.Map.GetObjects(global.Above()).Any() && this.Map.GetBlock(global.Above()) == BlockDefOf.Air;
            return !this.Map.GetObjects(global.Above()).Any(o => o.IsPlant()) && this.Map.GetBlock(global.Above()) == BlockDefOf.Air;
        }
        bool IsSaturated()
        {
            return this.CurrentDensity >= this.Town.Map.PlantDensityTarget;
        }
       
        private float GetCurrentPlantDensity(int totalFertileCells)
        {
            var plants = this.Map.GetObjects().Where(o => o.IsPlant()).Count();
            return plants / (float)totalFertileCells;
        }

        int GetTotalFertileCells()
        {
            var total = this.Map.GetAllCells().Where(c => c.Fertility > 0).Count();//.Aggregate((a, b) => a + b);
            return total;
        }
        protected override void AddSaveData(SaveTag tag)
        {
            tag.Add(this.DensityValidated.Save("Validated"));
            tag.Add(this.CurrentDensity.Save("CurrentDensity"));
            tag.Add(this.TotalFertileCells.Save("TotalFertileCells"));
            tag.Add(this.TotalFertileCellsTemp.Save("TotalFertileCellsTemp"));
            tag.Add(this.CycleIndex.Save("CycleIndex"));
        }
        public override void Load(SaveTag tag)
        {
            tag.TryGetTagValue("Validated", out this.DensityValidated);
            tag.TryGetTagValue("CurrentDensity", out this.CurrentDensity);
            tag.TryGetTagValue("TotalFertileCells", out this.TotalFertileCells);
            tag.TryGetTagValue("TotalFertileCellsTemp", out this.TotalFertileCellsTemp);
            tag.TryGetTagValue("CycleIndex", out this.CycleIndex);
        }
        //public void GeneratePlant()
        //{
        //    var map = this.Map as StaticMap;
        //    var rand = map.Random;
        //    var num = this.Map.ActiveChunks.Count;
        //    for (int i = 0; i < num; i++)
        //    {
        //        if (rand.Next(10) > 1)
        //            continue;
        //        var x = rand.Next(0, map.Size.Blocks);
        //        var y = rand.Next(0, map.Size.Blocks);
        //        var z = map.GetHeightmapValue(x, y);
        //        var cell = map.GetCell(x, y, z);
        //        var block = cell.Block;// map.GetBlock(x, y, z);
        //        if (
        //            //block == Block.Soil ||
        //            //block == Block.Grass)
        //            block.GetMaterial(cell.BlockData) == Components.Materials.Material.Soil)
        //        {
        //            var plant = rand.Next(2) < 1 ? GameObject.Objects[GameObject.Types.Tree].Clone() : GameObject.Objects[GameObject.Types.BerryBush].Clone();
        //            map.Net.Spawn(plant, new Vector3(x, y, z + 1));
        //        }
        //    }
        //}
    }
}
