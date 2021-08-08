using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Start_a_Town_.Towns;
using Start_a_Town_.UI;

namespace Start_a_Town_
{
    public class StorageManager : TownComponent
    {
        string _name = "Storage";
        public override string Name => _name;
        ObservableCollection<ItemMaterialAmount> CacheObservable = new();
        Dictionary<ItemDef, Dictionary<MaterialDef, ItemMaterialAmount>> Cache = new();
        ObservableDictionary<StorageItemBranch, ObservableDictionary<MaterialDef, StorageItemLeaf>> CacheObservableNew = new();
        StorageItemBranch RootNode = new();
        static readonly int TicksPerUpdate = Engine.TicksPerSecond;
        int UpdateTimer;

        public StorageManager(Town town) : base(town)
        {
        }

        public override void Update()
        {
            if (this.UpdateTimer-- <= 0)
            {
                this.UpdateTimer = TicksPerUpdate;
                foreach (var s in this.Town.ZoneManager.GetZones<Stockpile>())
                    s.CacheContentsNew();
                this.Refresh();
            }
        }
        private void Refresh()
        {
            var stockpiles = this.Town.ZoneManager.GetZones<Stockpile>();
            var sum = stockpiles
                .SelectMany(s => s.CacheObservable)
                .GroupBy(i => i.Def)
                .Select(o => new { def = o.Key, matSums = o
                    .GroupBy(oo => oo.PrimaryMaterial)
                    .ToDictionary(m => m.Key, m => m.Sum(o => o.StackSize)) })
                .ToDictionary(o => o.def, o => o.matSums);


            foreach (var def in this.Cache.ToList())
            {
                if (!sum.ContainsKey(def.Key))
                {
                    foreach (var i in this.Cache[def.Key])
                    {
                        this.CacheObservable.Remove(i.Value);
                        this.RootNode.Remove(def.Key);
                    }
                    this.Cache.Remove(def.Key);
                }
                else
                {
                    var mats = sum[def.Key];
                    foreach (var mat in def.Value.ToList())
                    {
                        var itemAmounts = this.Cache[def.Key];
                        if (!mats.TryGetValue(mat.Key, out var amount))
                        {
                            var cached = itemAmounts[mat.Key];
                            this.CacheObservable.Remove(cached);
                            this.RootNode[def.Key].Remove(mat.Key);
                            itemAmounts.Remove(mat.Key);
                        }
                    }
                }
            }

            foreach (var iDef in sum)
            {
                if (!this.Cache.TryGetValue(iDef.Key, out var mats))
                {
                    mats = new();
                    this.Cache.Add(iDef.Key, mats);
                    this.RootNode.Add(iDef.Key);
                }
                foreach(var mat in iDef.Value)
                {
                    if (!mats.TryGetValue(mat.Key, out var itemAmount))
                    {
                        itemAmount = new(iDef.Key, mat.Key, mat.Value);
                        mats.Add(mat.Key, itemAmount);
                        this.CacheObservable.Add(itemAmount);
                        this.RootNode[iDef.Key].Add(itemAmount);
                    }
                    else
                    {
                        itemAmount.Amount = mat.Value;
                        this.RootNode[iDef.Key][mat.Key].Item.Amount = mat.Value;
                    }
                }
            }
        }
        internal override void OnHudCreated(Hud hud)
        {
            hud.AddControls(getGui());
        }
        Control getGui()
        {
            var list = new ListCollapsibleObservable(this.RootNode, true);
            return list;

            var table = new TableObservable<ItemMaterialAmount>()
                .AddColumn("name", 96, i => i.GetListControlGui());
            table.Bind(this.CacheObservable);
            return table;
        }
        class StorageItemBranch : IListCollapsibleDataSourceObservable
        {
            public StorageItemBranch this[ItemDef branch] => (StorageItemBranch)this.Branches.First(c => ((StorageItemBranch)c).ItemDef == branch);
            public StorageItemLeaf this[MaterialDef leaf] => (StorageItemLeaf)this.Leafs.First(c => ((StorageItemLeaf)c).Key == leaf);

            ItemDef ItemDef;
            readonly HashSet<StorageItemLeaf> InnerItems = new();

            readonly ObservableCollection<IListCollapsibleDataSourceObservable> Branches = new();
            readonly ObservableCollection<IListable> Leafs = new();

            public string Label => this.ItemDef.Label;

            public ObservableCollection<IListCollapsibleDataSourceObservable> ListBranches => this.Branches;// new(this.Branches.Cast<IListCollapsibleDataSourceObservable>());
            public ObservableCollection<IListable> ListLeafs => this.Leafs;// new(this.Leafs.Cast<IListable>());

            public bool Remove(ItemDef item)
            {
                return this.Branches.Remove(this.Branches.First(i => ((StorageItemBranch)i).ItemDef == item));
            }
            public void Add(ItemDef item)
            {
                this.Branches.Add(new StorageItemBranch() { ItemDef = item });
            }
            public bool Remove(MaterialDef item)
            {
                var leaf = this.Leafs.First(i => ((StorageItemLeaf)i).Key == item);
                this.InnerItems.Remove(leaf as StorageItemLeaf);
                return this.Leafs.Remove(leaf);
            }
            public void Add(ItemMaterialAmount item)
            {
                var leaf = new StorageItemLeaf() { Key = item.Material, Item = item };
                this.InnerItems.Add(leaf);
                this.Leafs.Add(leaf);
            }
            public Control GetGui()
            {
                return new ListCollapsibleObservable(this, true);
            }

            public Control GetListControlGui()
            {
                return new Label(() => $"{this.InnerItems.Sum(l => l.Item.Amount)}x {this.Label}");
                //return new Label(() => $"{this.Leafs.Sum(l => (l as StorageItemLeaf).Item.Amount)}x {this.Label}");
                return null;// new Label(this.ItemDef.Label);
            }
            public override string ToString()
            {
                return this.ItemDef is ItemDef def ? $"{def.Label}: {this.Leafs.Count}" : $"Root: {this.Branches.Count}";
            }
        }
        
        class StorageItemLeaf : IListable
        {
            public MaterialDef Key;
            public ItemMaterialAmount Item;

            public string Label => this.Item.Label;

            public Control GetListControlGui()
            {
                return this.Item.GetListControlGui();
            }
            public override string ToString()
            {
                return this.Item.ToString();
            }
        }
    }
}
