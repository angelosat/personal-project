using Microsoft.Xna.Framework;
using Start_a_Town_.Components;
using Start_a_Town_.UI;
using Start_a_Town_.Net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Start_a_Town_
{
    public class GrowingZone : Zone, IContextable, ISelectable
    {
        [EnsureStaticCtorCall]
        static class Syncing
        {
            static readonly int pSync;
            static Syncing()
            {
                pSync = Network.RegisterPacketHandler(Sync);
            }
            public static void SyncProperties(GrowingZone zone)
            {
                if (zone.Net is Client)
                    return;

                var w = zone.Map.Net.GetOutgoingStream();
                w.Write(pSync);
                w.Write(zone.ID);
                w.Write(zone.Tilling);
                w.Write(zone.Planting);
                w.Write(zone.Harvesting);

            }
            static void Sync(INetwork net, BinaryReader r)
            {
                var zone = net.Map.Town.ZoneManager.GetZone<GrowingZone>(r.ReadInt32());
                zone.Tilling = r.ReadBoolean();
                zone.Planting = r.ReadBoolean();
                zone.Harvesting = r.ReadBoolean();
            }
        }

        internal bool IsValidTilling(IntVec3 global)
        {
            return this.CachedTilling.Contains(global);
        }

        bool _harvesting = true, _planting = true, _tilling = true;
        public bool Harvesting
        {
            get => this._harvesting;
            set
            {
                this._harvesting = !this._harvesting;
                Syncing.SyncProperties(this);
            }
        }
        public bool Planting
        {
            get => this._planting;
            set
            {
                this._planting = !this._planting;
                Syncing.SyncProperties(this);
            }
        }
        public bool Tilling
        {
            get => this._tilling;
            set
            {
                this._tilling = !this._tilling;
                Syncing.SyncProperties(this);
            }
        }

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
            w.Write(this._tilling);
            w.Write(this._planting);
            w.Write(this._harvesting);
            this.Plant.Write(w);
        }
        protected override void ReadExtra(BinaryReader r)
        {
            this._tilling = r.ReadBoolean();
            this._planting = r.ReadBoolean();
            this._harvesting = r.ReadBoolean();
            this.Plant = r.ReadDef<PlantProperties>();
        }

        protected override void LoadExtra(SaveTag tag)
        {
            tag.TryGetTagValueNew("Tilling", ref this._tilling);
            tag.TryGetTagValueNew("Planting", ref this._planting);
            tag.TryGetTagValueNew("Harvesting", ref this._harvesting);
            tag.TryLoadDef("Plant", ref this.Plant);
        }
        protected override void SaveExtra(SaveTag tag)
        {
            tag.Add(this.Tilling.Save("Tilling"));
            tag.Add(this.Planting.Save("Planting"));
            tag.Add(this.Harvesting.Save("Harvesting"));
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

        public IEnumerable<IntVec3> GetSowingPositions()
        {
            if (!this.Valid)
                this.Validate();
            foreach (var pos in this.CachedSowing)
                yield return pos;
        }

        public IEnumerable<IntVec3> GetTillingPositions()
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
                //else if (block.GetMaterial(cellData) == MaterialDefOf.Soil)
                else if (cell.Material == MaterialDefOf.Soil)
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
            return this.Plant is not null && item.IsSeedFor(this.Plant);
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
                GrowingZone growzone = null;
                var box = new GroupBox(300, 200);
                box.AddControlsVertically(
                    //new CheckBoxNew("Tilling", () => Syncing.SyncProperties(growzone), () => growzone.Tilling),
                    //new CheckBoxNew("Planting", () => Syncing.SyncProperties(growzone), () => growzone.Planting),
                    //new CheckBoxNew("Harvesting", () => Syncing.SyncProperties(growzone), () => growzone.Harvesting)
                    new CheckBoxNew("Tilling", () => growzone.Tilling = !growzone.Tilling, () => growzone.Tilling),
                    new CheckBoxNew("Planting", () => growzone.Planting = !growzone.Planting, () => growzone.Planting),
                    new CheckBoxNew("Harvesting", () => growzone.Harvesting = !growzone.Harvesting, () => growzone.Harvesting)
                    );
                var win = box.ToWindow();
                win.SetGetDataAction(o =>
                {
                    growzone = o as GrowingZone;
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

        public override bool Accepts(Entity obj, IntVec3 pos)
        {
            if (this.Plant is null)
                return false;
            if (obj.Def != ItemDefOf.Seeds)
                return false;
            if (!this.CachedSowing.Contains(pos))
                return false;
            return obj.IsSeedFor(this.Plant);
        }
    }
}
