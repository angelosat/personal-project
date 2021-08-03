﻿using Microsoft.Xna.Framework;
using Start_a_Town_.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Start_a_Town_
{
    record ItemFilter
    {
        public readonly ItemDef Item;
        public bool Enabled;
        readonly HashSet<MaterialDef> DisallowedMaterials = new();
        public ItemFilter(ItemDef item, bool enabled = true)
        {
            this.Item = item;
            this.Enabled = enabled;
        }
        public bool IsAllowed(MaterialDef mat)
        {
            return this.Enabled && !this.DisallowedMaterials.Contains(mat);
        }
        internal ItemFilter SetAllow(MaterialDef m, bool allow)
        {
            if (allow)
                this.DisallowedMaterials.Remove(m);
            else
                this.DisallowedMaterials.Add(m);
            return this;
        }

        internal void Toggle(MaterialDef mat = null)
        {
            if (mat == null)
                this.Enabled = !this.Enabled;
            else
            {
                if (this.DisallowedMaterials.Contains(mat))
                    this.DisallowedMaterials.Remove(mat);
                else
                    this.DisallowedMaterials.Add(mat);
            }
        }
    }

    public partial class Stockpile : Zone, IStorage, IContextable
    {
        //{
        //    public ItemDef Item;
        //    HashSet<MaterialDef> AllowedMaterials;
        //}
        internal static void Init()
        {
            Packets.Init();
        }

        internal void ToggleFilter(ItemDef item, MaterialDef mat = null)
        {
            var record = this.Allowed[item];
            if (mat == null)
                record.Toggle();
            else
                record.Toggle(mat);
        }
        internal void ToggleFilter(ItemCategory cat)
        {
            var byCategory = this.Allowed.Values.ToLookup(r => r.Item.Category);
            var records = byCategory[cat];
            var minor = records.GroupBy(a => a.Enabled).OrderBy(a => a.Count()).First();
            foreach (var f in minor)
                f.Toggle();
        }
        StorageSettings IStorage.Settings => this.Settings;
        public override ZoneDef ZoneDef => ZoneDefOf.Stockpile;
        public StorageSettings Settings = new();
        public int Priority => this.Settings.Priority.Value;
        static IListCollapsibleDataSource DefaultFiltersNew;// = InitFilters();
        Dictionary<ItemDef, ItemFilter> Allowed = new();

        public override string UniqueName => $"Zone_Stockpile_{this.ID}";

        public override string GetName()
        {
            return this.Name;
        }

        public void CacheContents()
        {
            this.CacheContentsNew();
        }
        public void CacheContentsNew()
        {
            this.Cache.Clear();
            foreach (var pos in this.Positions)
                this.Cache.AddRange(this.Map.GetObjects(pos.Above).Where(i => i.IsStockpilable()));
        }
        public IEnumerable<GameObject> GetContentsNew()
        {
            foreach (var i in this.Cache)
                yield return i;
        }
        public List<GameObject> GetContents()
        {
            var contents = new List<GameObject>();
            foreach (var pos in this.Positions)
                contents.AddRange(this.Town.Map.GetObjects(pos + IntVec3.UnitZ));
            return contents;
        }

        readonly List<GameObject> Cache = new();

        public List<GameObject> ScanExistingStoredItems()
        {
            List<GameObject> list = new List<GameObject>();
            var objects = this.Town.Map.GetObjects();
            foreach (var pos in this.Positions)
                list.AddRange(from obj in objects where this.Accepts(obj) where obj.Global - Vector3.UnitZ == (Vector3)pos select obj); // TODO: this is shit
            return list;
        }
        public bool Accepts(ItemDef item, MaterialDef mat)
        {
            var record = this.Allowed[item];
            return record.IsAllowed(mat);
        }
        public override bool Accepts(Entity obj, IntVec3 pos)
        {
            if (!this.Positions.Contains(pos))
                return false;
            return this.Accepts(obj);
        }
        public bool Accepts(Entity obj)
        {
            return this.Allowed[obj.Def].IsAllowed(obj.PrimaryMaterial);
            //return this.DefaultFilters.Filter(obj);
        }
        internal bool Accepts(GameObject obj)
        {
            return obj is Entity item ? this.Accepts(item) : false;
        }
        public bool CanAccept(GameObject item)
        {
            return this.Accepts(item) && this.GetAvailableCells().Any();
        }

        public IEnumerable<IntVec3> GetAvailableCells()
        {
            var emptyCells =
                this.Positions
                .Where(p => this.Town.ReservationManager.CanReserve(p.Above))
                .Where(p => !this.Town.Map.GetObjects(p.Above).Any())
                .Select(p => p.Above);
            return emptyCells;
        }
        

        public IEnumerable<TargetArgs> DistributeToStorageSpotsNewLazy(GameObject actor, GameObject obj)
        {
            var emptyCells = new List<Vector3>();
            foreach (var pos in this.Positions)
            {
                var above = pos.Above;
                var itemsInCell = this.Map.GetObjects(above);
                if (!itemsInCell.Any())
                {
                    emptyCells.Add(above);
                    continue;
                }
                foreach (var item in itemsInCell)
                {
                    if (!this.Accepts(item))
                        continue;
                    if (!item.CanAbsorb(obj))
                        continue;
                    yield return new TargetArgs(item);
                }
            }
            foreach (var cell in emptyCells)
            {
                yield return new TargetArgs(this.Map, cell);
            }
        }
        public Dictionary<TargetArgs, int> DistributeToStorageSpotsNew(Actor actor, GameObject obj, out int maxamount)
        {
            var valid = new Dictionary<TargetArgs, int>();
            var currentSimilarContents = this.ScanExistingStoredItems().Where(o => o.CanAbsorb(obj) && this.Accepts(o) && o.StackSize < o.StackMax);

            maxamount = 0;
            foreach (var item in currentSimilarContents)
            {
                if (!actor.CanReserve(item))
                    continue;
                var validAmount = item.StackMax - item.StackSize;
                valid.Add(new TargetArgs(item), validAmount);
                maxamount += validAmount;
            }
            var emptyCells =
                this.Positions
                .Where(pos => !this.Town.Map.GetObjects(pos.Above)
                    .Where(t => t != actor)
                    .Any())
                .Select(p => p.Above).ToList();

            foreach (var pos in emptyCells)
            {
                if (!actor.CanReserve(pos))
                    continue;
                valid.Add(new TargetArgs(pos), obj.StackMax);
                maxamount += obj.StackMax;
            }
            return valid;
        }

        public IEnumerable<TargetArgs> GetPotentialHaulTargets(Actor actor, GameObject item)
        {
            foreach (var target in this.DistributeToStorageSpotsNewLazy(actor, item))
                yield return target;
        }
        public Dictionary<TargetArgs, int> GetPotentialHaulTargets(Actor actor, GameObject item, out int maxAmount)
        {
            return this.DistributeToStorageSpotsNew(actor, item, out maxAmount);
        }

        public void FilterToggle(params StorageFilter[] filters)
        {
            foreach (var n in filters)
                this.ToggleFilter(n);
        }

        public void ToggleFilter(StorageFilter filter)
        {
            this.Settings.Toggle(filter);
        }

        internal override void OnBlockChanged(IntVec3 global)
        {
            var below = global.Below;

            if (this.Positions.Contains(global))
            {
                if (!Block.IsBlockSolid(this.Town.Map, global))
                {
                    this.RemovePosition(global);
                    return;
                }
            }
            else if (this.Positions.Contains(below))
            {
                if (!this.Map.IsAir(global))
                {
                    this.RemovePosition(below);
                    return;
                }
            }
        }
        public Stockpile()
        {
            InitFilters();
        }
        public Stockpile(ZoneManager manager) : base(manager)
        {
            InitFilters();
        }
        public Stockpile(ZoneManager manager, IEnumerable<IntVec3> positions)
            : base(manager, positions)
        {
            InitFilters();
        }
        public void GetContextActions(GameObject playerEntity, ContextArgs a)
        {
        }

        public bool IsValidStorage(Entity item, TargetArgs target, int amount)
        {
            if (!this.Accepts(item))
                return false;
            var itemsOnCell = this.Town.Map.GetObjects(target.Global);

            switch (target.Type)
            {
                case TargetType.Position:

                    break;

                case TargetType.Entity:
                    if (!itemsOnCell.Contains(target.Object))
                        throw new Exception();
                    if (target.Object.StackSize + amount > target.Object.StackMax)
                        throw new Exception();
                    break;

                default:
                    break;
            }

            return true;
        }

        public IEnumerable<Vector3> GetPositionsLazy()
        {
            foreach (var pos in this.Positions)
                yield return pos;
        }

        public override void GetSelectionInfo(IUISelection panel)
        {
            panel.AddTabAction("Stockpile", this.ToggleFiltersUI);

        }
        static Window WindowFilters;
        static Control FiltersGui;
        private void ToggleFiltersUI()
        {
            // TODO: update controls when selecting another stockpile
            if (WindowFilters is not null && WindowFilters.Tag != this && WindowFilters.IsOpen)
            {
                WindowFilters.Client.ClearControls();
                WindowFilters.Client.AddControls(GetGUI());
                WindowFilters.SetTitle($"{this.UniqueName} settings");
                WindowFilters.SetTag(this);
                return;
            }
            if (WindowFilters is null)
            {
                WindowFilters =
                    GetGUI()
                    .ToWindow("Stockpile settings");
            }
            WindowFilters.SetTitle($"{this.UniqueName} settings");
            WindowFilters.SetTag(this);
            WindowFilters.Toggle();

            WindowFilters.SetOnSelectedTargetChangedAction(t =>
            {
                if (this.Town.ZoneManager.GetZoneAt<Stockpile>(t.Global) is Stockpile newStockpile)
                {
                    WindowFilters.SetTitle($"{newStockpile.UniqueName} settings");
                    WindowFilters.SetTag(newStockpile);
                    DefaultFiltersNew.SetOwner(newStockpile);
                }
            });
        }
        static Control GetGUI()
        {
            if (FiltersGui is not null)
                return FiltersGui;

            var box = new GroupBox();
            box.AddControlsVertically(
                new GroupBox()
                    .AddControlsHorizontally(new ComboBoxNewNew<StoragePriority>(StoragePriority.All, 128, p => $"Priority: {p}", p => $"{p}", syncPriority, () => DefaultFiltersNew.Owner.Settings.Priority)),
                DefaultFiltersNew
                    .GetGui()
                    .ToPanelLabeled("Fitlers"));
            
            FiltersGui = box;
            return box;

            void syncPriority(StoragePriority p)
            {
                Packets.SyncPriority(DefaultFiltersNew.Owner, p);
            }
        }
        
        IListCollapsibleDataSource InitFilters()
        {
            var cats = Def.Database.Values.OfType<ItemDef>().GroupBy(d => d.Category);

            var all = new StorageFilterCategoryNewNew("All") { Owner = this };
            foreach (var cat in cats)
            {
                if (cat.Key == null)
                    continue;
                var c = new StorageFilterCategoryNewNew(cat.Key.Label) { Category = cat.Key };
                all.AddChildren(c);
                foreach (var def in cat)
                {
                    var record = new ItemFilter(def);
                    this.Allowed.Add(def, record);
                    if (def.DefaultMaterialType != null)
                        c.AddChildren(new StorageFilterCategoryNewNew(def.Label) { Item = def }.AddLeafs(def.DefaultMaterialType.SubTypes.Select(m => new StorageFilterNewNew(def, m))));
                    else
                        c.AddLeafs(new StorageFilterNewNew(def));
                }
            }
            DefaultFiltersNew = all;
            return all;
        }

        public void ToggleItemFiltersCategories(int[] categoryIndices)
        {
            //var indices = categoryIndices;
            //foreach (var i in indices)
            //{
            //    var c = this.DefaultFilters.GetNodeByIndex(i);
            //    var all = c.GetAllDescendantLeaves();
            //    var minor = all.GroupBy(a => a.Enabled).OrderBy(a => a.Count()).First();
            //    foreach (var f in minor)
            //        f.Enabled = !minor.Key;
            //}
        }
        public void ToggleItemFilters(int[] gameObjects)
        {
            //var indices = gameObjects;
            //foreach (var i in indices)
            //{
            //    var f = this.DefaultFilters.GetLeafByIndex(i);
            //    f.Enabled = !f.Enabled;
            //}
        }
    }
}
