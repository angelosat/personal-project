using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_
{
    public class ItemCategory : Def
    {
        public List<StorageFilter> Filters = new();
        public List<StatNewDef> Stats = new();
        public string Label;
        ItemCategory(string label) : base("StorageCategory:"+label)
        {

            this.Label = label;
        }
        public void Add(StorageFilter filter)
        {
            this.Filters.Add(filter);
        }
        public void Add(GameObject o)
        {
            var cat = new StorageFilter(o.Name, obj => obj.ID == o.ID);
            this.Filters.Add(cat);
            o.StorageCategory = cat;
        }
        public void Add(int oID)
        {
            var o = GameObject.Objects[oID];
            var cat = new StorageFilter(o.Name, obj => obj.ID == o.ID);
            this.Filters.Add(cat);
            o.StorageCategory = cat;
        }
        public ItemCategory AddStats(params StatNewDef[] stats)
        {
            this.Stats.AddRange(stats);
            return this;
        }
        static public readonly ItemCategory Unlisted = new("Unlisted");
        static public readonly ItemCategory Tools = new ItemCategory("Tools").AddStats(StatDefOf.ToolStatPackage);
        static public readonly ItemCategory Wearables = new("Wearables");
        static public readonly ItemCategory RawMaterials = new("RawMaterials");
        static public readonly ItemCategory Manufactured = new("Manufactured");
        static public readonly ItemCategory FoodRaw = new("FoodRaw");
        static public readonly ItemCategory FoodCooked = new("FoodCooked");

        static public readonly List<ItemCategory> All = new()
        {
            Unlisted, Tools, Wearables, RawMaterials, Manufactured,FoodRaw, FoodCooked
        };
        static ItemCategory()
        {
            //Def.Register(Unlisted);
            Def.Register(Tools);
            Def.Register(Wearables);
            Def.Register(RawMaterials);
            Def.Register(Manufactured);
            Def.Register(FoodRaw);
        }
        
        //static public List<StorageFilter> CreateFilterSet()
        //{
        //    return 
        //        (from c in All
        //        from f in c.Filters
        //        select f).ToList();
        //}
        public override string ToString()
        {
            return this.Name;
        }
        static public StorageFilter Find(string filterName)
        {
            foreach(var c in All)
            {
                var f = c.Filters.FirstOrDefault(_f => _f.Name == filterName);
                if (f != null)
                    return f;
            }
            throw new Exception();
        }
    }
}
