using Microsoft.Xna.Framework;
using Start_a_Town_.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Start_a_Town_
{
    public partial class Stockpile : Zone, IStorageNew, IStorage, IContextable
    {
        internal static void Init()
        {
            Packets.Init();
        }
        
        public int StorageID { get; }
        public override ZoneDef ZoneDef => ZoneDefOf.Stockpile;
        public StorageSettings Settings { get; } = new();
        public int Priority => this.Settings.Priority.Value;
        static StorageFilterCategoryNewNew FiltersView = InitFilters();
        readonly Dictionary<ItemDef, ItemFilter> Allowed = new();
        public override string UniqueName => $"Zone_Stockpile_{this.ID}";

        public Stockpile()
        {
            this.InitStorageFilters();
        }
        public Stockpile(ZoneManager manager) : base(manager)
        {
            this.InitStorageFilters();
        }
        public Stockpile(ZoneManager manager, IEnumerable<IntVec3> positions)
            : base(manager, positions)
        {
            this.InitStorageFilters();
        }

        internal void ToggleFilter(ItemDef item, Def variator)
        {
            if (variator is not IItemDefVariator)
                throw new Exception();
            var record = this.Allowed[item];
            record.Toggle(variator);
        }
        internal void ToggleFilter(ItemDef item, MaterialDef mat)
        {
            var record = this.Allowed[item];
            record.Toggle(mat);
        }
        internal void ToggleFilter(ItemDef item)
        {
            var record = this.Allowed[item];
            //record.Toggle();
            if (item.StorageFilterVariations is not null)
            {
                //foreach (var m in item.StorageFilterVariations)
                //    record.Toggle(m as Def);
                var minor = item.StorageFilterVariations.GroupBy(a => record.IsAllowed(a as Def)).OrderBy(a => a.Count()).First();
                foreach (var m in minor)
                    record.Toggle(m as Def);
            }
            else if (item.DefaultMaterialType is not null)
            {
                //foreach (var m in item.DefaultMaterialType.SubTypes)
                //    record.Toggle(m);
                var minor = item.DefaultMaterialType.SubTypes.GroupBy(a => record.IsAllowed(a)).OrderBy(a => a.Count()).First();
                foreach (var m in minor)
                    record.Toggle(m);
            }
            else
                record.Toggle();
        }
        internal void ToggleFilter(ItemCategory cat)
        {
            IEnumerable<ItemFilter> records = null;
            if (cat is null)
                records = this.Allowed.Values;
            else
            {
                var byCategory = this.Allowed.Values.ToLookup(r => r.Item.Category);
                records = byCategory[cat];
            }
            var catNode = FiltersView.FindNode(cat);
            var leafs = catNode.GetAllDescendantLeaves();
            var leafsMinor = leafs.GroupBy(l => l.Enabled).OrderBy(l => l.Count()).First();
            foreach(var l in leafsMinor)
            {
                if (l.Variation is not null)
                    this.ToggleFilter(l.Item, l.Variation);
                else if (l.Material is not null)
                    this.ToggleFilter(l.Item, l.Material);
                else
                    this.ToggleFilter(l.Item);
            }
        }

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
        public bool Accepts(ItemDef item, Def v)
        {
            var record = this.Allowed[item];
            return record.IsAllowed(v);
        }
        public bool Accepts(ItemDef item, MaterialDef mat)
        {
            var record = this.Allowed[item];
            return record.IsAllowed(mat);
        }
        public bool Accepts(ItemDef item, MaterialDef mat, Def variation)
        {
            var record = this.Allowed[item];
            return record.IsAllowed(mat) && record.IsAllowed(variation);
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
            return obj is Entity item && this.Accepts(item);
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
                    if (!item.IsHaulable)
                        continue;
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
            FiltersView.SetOwner(this);

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
                    FiltersView.SetOwner(newStockpile);
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
                    .AddControlsHorizontally(new ComboBoxNewNew<StoragePriority>(StoragePriority.All, 128, p => $"Priority: {p}", p => $"{p}", syncPriority, () => FiltersView.Owner.Settings.Priority)),
                FiltersView
                    .GetGui()
                    .ToPanelLabeled("Fitlers"));
            
            FiltersGui = box;
            return box;

            void syncPriority(StoragePriority p)
            {
                Packets.SyncPriority(FiltersView.Owner, p);
            }
        }
        void InitStorageFilters()
        {
            foreach (var c in FiltersView.GetAllDescendantLeaves().GroupBy(l => l.Item))
                this.Allowed.Add(c.Key, new(c.Key));
        }
        static StorageFilterCategoryNewNew InitFilters()
        {
            var cats = Def.Database.Values.OfType<ItemDef>().GroupBy(d => d.Category);

            var all = new StorageFilterCategoryNewNew("All");// { Owner = this };
            foreach (var cat in cats)
            {
                if (cat.Key == null)
                    continue;
                var c = new StorageFilterCategoryNewNew(cat.Key.Label) { Category = cat.Key };
                all.AddChildren(c);
                foreach (var def in cat)
                {
                    var record = new ItemFilter(def);
                    //this.Allowed.Add(def, record);
                    if (def.DefaultMaterialType != null)
                        c.AddChildren(new StorageFilterCategoryNewNew(def.Label) { Item = def }.AddLeafs(def.DefaultMaterialType.SubTypes.Select(m => new StorageFilterNewNew(def, m))));
                    //else if(def.GetSpecialFilters() is IEnumerable<StorageFilterNewNew> filters)
                    //    c.AddLeafs(filters);
                    else if(def.GetSpecialFilter() is StorageFilterCategoryNewNew filter)
                        c.AddChildren(filter);
                    else
                        c.AddLeafs(new StorageFilterNewNew(def));
                }
            }
            FiltersView = all;
            return all;
        }
        [Obsolete]
        public void ToggleItemFiltersCategories(int[] categoryIndices)
        {
            throw new Exception();
        }
        [Obsolete]
        public void ToggleItemFilters(int[] gameObjects)
        {
            throw new Exception();
        }
        protected override void SaveExtra(SaveTag tag)
        {
            this.Allowed.Values.SaveNewBEST(tag, "Filters");
        }
        protected override void LoadExtra(SaveTag tag)
        {
            tag.TryGetTag("Filters", t =>
            {
                var list = t.LoadList<ItemFilter>();
                foreach (var r in list)
                {
                    if (r is null) // in case an itemdef has been changed/removed
                        continue;
                    this.Allowed[r.Item].CopyFrom(r);
                }
            });
        }
        protected override void WriteExtra(BinaryWriter w)
        {
            this.Allowed.Values.Sync(w);
        }
        protected override void ReadExtra(BinaryReader r)
        {
            this.Allowed.Values.Sync(r);
        }
    }
}
