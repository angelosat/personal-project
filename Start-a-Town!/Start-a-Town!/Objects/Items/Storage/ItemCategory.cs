using System.Collections.Generic;

namespace Start_a_Town_
{
    public class ItemCategory : Def
    {
        public List<StorageFilter> Filters = new();
        public List<StatDef> Stats = new();
        public string Label;
        ItemCategory(string label) : base("StorageCategory:"+label)
        {
            this.Label = label;
        }
        public ItemCategory AddStats(params StatDef[] stats)
        {
            this.Stats.AddRange(stats);
            return this;
        }
        static public readonly ItemCategory Unlisted = new("Unlisted");
        static public readonly ItemCategory Equipment = new ItemCategory("Equipment").AddStats(StatDefOf.ToolStatPackage);
        static public readonly ItemCategory Wearables = new("Wearables");
        static public readonly ItemCategory RawMaterials = new("RawMaterials");
        static public readonly ItemCategory Manufactured = new("Manufactured");
        static public readonly ItemCategory FoodRaw = new("FoodRaw");
        static public readonly ItemCategory FoodCooked = new("FoodCooked");

        static public readonly List<ItemCategory> All = new()
        {
            Unlisted, Equipment, Wearables, RawMaterials, Manufactured,FoodRaw, FoodCooked
        };
        static ItemCategory()
        {
            Register(Equipment);
            Register(Wearables);
            Register(RawMaterials);
            Register(Manufactured);
            Register(FoodRaw);
        }
        
        public override string ToString()
        {
            return this.Name;
        }
    }
}
