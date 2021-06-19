using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.Components;

namespace Start_a_Town_
{
    public class StorageFilter : Def, ILabeled
    {
        //public string Name;
        Func<Entity, bool> _Condition;
        public virtual string Label { get; set; }
        public StorageFilter(string name):base(name)
        {

        }
        public StorageFilter(string name, Func<Entity, bool> cond) : base(name)
        {
               this._Condition = cond;
        }
        public virtual bool Condition(Entity obj)
        {
            return this._Condition(obj);
        }
        public override string ToString()
        {
            return this.Name;
        }

        //static public readonly StorageFilter Food = new StorageFilter("Food", item => item.HasComponent<ConsumableComponent>());
        //static public readonly StorageFilter RawMaterial = new StorageFilter("RawMaterial", item => item.Def.StorageCategory == StorageCategory.Resources);// is RawMaterialDef);
        //static public readonly StorageFilter Tools = new StorageFilter("Tools", item => item.Def is ItemToolDef);

        static StorageFilter CreateFromCategory(ItemCategory cat)
        {
            return new StorageFilter("StorageFilterFromCategory:" + cat.Name, item => item.Def.Category == cat) { Label = cat.Name };
        }
        static public StorageFilter CreateFromItemDef(ItemDef def)
        {
            return new StorageFilter("StorageFilterFromItemDef:" + def.Name, item => item.Def== def) { Label = def.Name };
        }
        static public HashSet<StorageFilter> CreateFilterSet()
        {
            //var set = new HashSet<StorageFilter>((from c in All
            //                                      from f in c.Filters
            //                                      select f));

            var filters = Def.Database.Values.OfType<StorageFilter>();
            return new HashSet<StorageFilter>(filters);

            //return new HashSet<StorageFilter>() {
            //    StorageFilter.Food,
            //    StorageFilter.RawMaterial,
            //};
        }
        static StorageFilter()
        {
            //var categories = Def.Database.Values.OfType<StorageCategory>().ToList();
            //foreach(var cat in categories)
            //{
            //    Register(CreateFromCategory(cat));
            //}

            var itemByDef = Def.Database.Values.OfType<ItemDef>().GroupBy(d => d.Category);
            foreach(var group in itemByDef)
            {
                if (group.Key == null)
                    continue;
                foreach (var def in group)
                {
                    var filter = CreateFromItemDef(def);
                    Register(filter);
                    group.Key.Filters.Add(filter);
                }
            }
        }
    }
}
