using Microsoft.Xna.Framework;
using Start_a_Town_.Components;
using Start_a_Town_.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Start_a_Town_
{
    public class GrowingZone : Zone, IContextable, ISelectable
    {
        public bool Harvesting = true, Planting = true;
        public float HarvestThreshold = 1;
        public override string UniqueName => $"Zone_Growing_{this.ID}";
        public ItemDef SeedType = PlantDefOf.Bush;
        public PlantProperties Plant = PlantProperties.Berry;
        readonly HashSet<IntVec3> CachedTilling = new();
        readonly HashSet<IntVec3> CachedSowing = new();
        bool Valid;

        public GrowingZone(BinaryReader r)
            : base()
        {
            this.Read(r);
        }

        public GrowingZone(ZoneManager manager) : base(manager)
        {
        }
        public GrowingZone(ZoneManager manager, IEnumerable<IntVec3> positions)
            : base(manager, positions)
        {
        }
        public override ZoneDef ZoneDef => ZoneDefOf.Growing;

        protected override void WriteExtra(BinaryWriter w)
        {
            this.Plant.Write(w);
        }
        protected override void ReadExtra(BinaryReader r)
        {
            this.Plant = r.ReadDef<PlantProperties>();
        }

        protected override void LoadExtra(SaveTag tag)
        {
            tag.TryGetTagValue("Harvesting", out this.Harvesting);
            tag.TryGetTagValue("Planting", out this.Planting);
            tag.TryLoadDef<PlantProperties>("Plant", ref this.Plant);
        }
        protected override void SaveExtra(SaveTag tag)
        {
            tag.Add(this.Harvesting.Save("Harvesting"));
            tag.Add(this.Planting.Save("Planting"));
            tag.Add(this.Plant.Save("Plant"));
        }

        internal override void OnBlockChanged(IntVec3 global)
        {
            var below = global.Below;
            var map = this.Map;
            if (this.Positions.Contains(global))
            {
                if (!Block.IsBlockSolid(map, global) || map.GetBlockMaterial(global) != MaterialDefOf.Soil)
                {
                    this.RemovePosition(global);
                    return;
                }
            }
            else if (this.Positions.Contains(below))
            {
                if (!map.IsAir(global))
                {
                    this.RemovePosition(below);
                    return;
                }
            }
        }

        public IEnumerable<Vector3> GetSowingPositions()
        {
            if (!this.Valid)
                this.Validate();
            foreach (var pos in this.CachedSowing)
                yield return pos;
        }

        public IEnumerable<Vector3> GetTillingPositions()
        {
            if (!this.Valid)
                this.Validate();
            foreach (var pos in this.CachedTilling)
                yield return pos;
        }

        protected override void Validate()
        {
            this.Valid = true;
            this.CachedTilling.Clear();
            this.CachedSowing.Clear();
            foreach (var pos in this.Positions)
            {
                var cell = this.Town.Map.GetCell(pos);
                var block = cell.Block;
                var cellData = cell.BlockData;
                if (block == BlockDefOf.Farmland)
                {
                    if (!this.Town.Map.GetObjects(pos.Above).Any(o => o.IsPlant()))
                        if (!Blocks.BlockFarmland.IsSeeded(cellData))
                            this.CachedSowing.Add(pos);
                }
                else if (block.GetMaterial(cellData) == MaterialDefOf.Soil)
                {
                    if (!this.Town.Map.GetObjects(pos.Above).Any(o => o.IsPlant()))
                        this.CachedTilling.Add(pos);
                }
            }
        }

        public void GetContextActions(GameObject playerEntity, ContextArgs a) { }

        public static bool IsValidFarmPosition(MapBase map, Vector3 arg)
        {
            return
                Block.GetBlockMaterial(map, arg) == MaterialDefOf.Soil
                && map.GetBlock(arg + Vector3.UnitZ) == BlockDefOf.Air;
        }

        internal IEnumerable<GameObject> GetHarvestablePlantsLazy()
        {
            foreach (var pos in this.Positions)
            {
                var above = pos.Above;
                var grownPlants = this.Town.Map.GetObjects(above).OfType<Plant>().Where(p => p.FruitGrowth >= this.HarvestThreshold);
                foreach (var obj in grownPlants)
                    yield return obj;
            }
        }
        internal IEnumerable<GameObject> GetHarvestablePlants()
        {
            return this.Town.Map.GetObjects(this.Positions.Select(pos => (Vector3)pos.Above)).OfType<Plant>().Where(p => p.IsHarvestable);

        }
        internal IEnumerable<GameObject> GetChoppableTrees()
        {
            return this.Town.Map.GetObjects(this.Positions.Select(pos => (Vector3)pos.Above)).Where(TreeComponent.IsGrown);
        }
        public bool IsValidSeed(GameObject item)
        {
            return this.Plant is not null && item.GetComponent<SeedComponent>()?.Plant == this.Plant;
        }
        public override string GetName()
        {
            return this.Name;
        }
        public override IEnumerable<(string name, Action action)> GetInfoTabs()
        {
            yield return ("Plant", this.ToggleGui);
        }
        static Control gui;
        void ToggleGui()
        {
            (gui ??= createGui())
                .GetData(this)
                .Show();

            static Control createGui()
            {
                var box = new GroupBox(300, 200);
                var win = box.ToWindow();
                win.SetGetDataAction(o =>
                {
                    var growzone = o as GrowingZone;
                    win.SetTitle(growzone.Name);
                });
                win.SetOnSelectedTargetChangedAction(t =>
                {
                    if (t.Type != TargetType.Position)
                        return;
                    if (t.Map.Town.ZoneManager.GetZoneAt<GrowingZone>(t.Global) is not GrowingZone gz)
                        return;
                    win.GetData(gz);
                });
                return box.Window;
            }
        }
    }
}
