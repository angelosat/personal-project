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
                    foreach(var i in this.Cache[def.Key])
                        this.CacheObservable.Remove(i.Value);
                    this.Cache.Remove(def.Key);
                }
                else
                {
                    var mats = sum[def.Key];
                    foreach (var mat in def.Value)
                    {
                        var itemAmounts = this.Cache[def.Key];
                        if (!mats.TryGetValue(mat.Key, out var amount))
                        {
                            var cached = itemAmounts[mat.Key];
                            this.CacheObservable.Remove(cached);
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
                }
                foreach(var mat in iDef.Value)
                {
                    if(!mats.TryGetValue(mat.Key, out var itemAmount))
                    {
                        itemAmount = new(iDef.Key, mat.Key, mat.Value);
                        mats.Add(mat.Key, itemAmount);
                        this.CacheObservable.Add(itemAmount);
                    }
                    else
                        itemAmount.Amount = mat.Value;
                }
            }
        }
        internal override void OnHudCreated(Hud hud)
        {
            hud.AddControls(getGui());
        }
        Control getGui()
        {
            var table = new TableObservable<ItemMaterialAmount>()
                .AddColumn("name", 96, i => i.GetListControlGui());
            table.Bind(this.CacheObservable);
            return table;
        }
        //private void Refresh()
        //{
        //    var stockpiles = this.Town.ZoneManager.GetZones<Stockpile>();
        //    foreach(var st in stockpiles)
        //    {
        //        var cont = st.GetContentsNew();
        //        foreach(var item in cont)
        //        {
        //            var itemDef = item.Def;
        //            if(!this.Cache.TryGetValue(itemDef, out var itemDic))
        //            {
        //                itemDic = new();
        //                this.Cache.Add(itemDef, itemDic);
        //            }
        //            var mat = item.PrimaryMaterial;
        //            if(!itemDic.TryGetValue(mat, out var itemMatAmount))
        //            {
        //                itemMatAmount = new(itemDef, mat, 0);
        //                itemDic.Add(mat, itemMatAmount);
        //                this.CacheOvservable.Add(itemMatAmount);
        //            }
        //            itemMatAmount.Amount += item.StackSize;
        //        }
        //    }
        //}
    }
}
