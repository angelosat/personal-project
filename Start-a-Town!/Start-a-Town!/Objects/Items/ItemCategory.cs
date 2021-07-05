using System;

namespace Start_a_Town_
{
    [Obsolete]
    public class ItemCategoryOld
    {
        public string Name { get; set; }
        public ItemCategoryOld(string name)
        {
            this.Name = name;
        }

        static public readonly ItemCategoryOld Generic = new ItemCategoryOld("Generic");
        static public readonly ItemCategoryOld RawMaterial = new ItemCategoryOld("Raw Material");
        static public readonly ItemCategoryOld Weapon = new ItemCategoryOld("Weapon");
        static public readonly ItemCategoryOld Tool = new ItemCategoryOld("Tool");
        static public readonly ItemCategoryOld Facility = new ItemCategoryOld("Facility");
        static public readonly ItemCategoryOld Entity = new ItemCategoryOld("Entity");
        static public readonly ItemCategoryOld Vegetation = new ItemCategoryOld("Vegetation");
        static public readonly ItemCategoryOld Furniture = new ItemCategoryOld("Furniture");

    }
    public class ItemSubType
    {
        public string Name { get; set; }
        public ItemCategoryOld ItemType { get; set; }
        public ItemSubType(string name, ItemCategoryOld type)
        {
            this.Name = name;
            this.ItemType = type;
        }

        public override string ToString()
        {
            return ((this.ItemType == ItemCategoryOld.Generic) ? "" : this.ItemType.Name + ": ") + this.Name;
        }

        static public readonly ItemSubType Generic = new ItemSubType("Generic", ItemCategoryOld.Generic); 

        static public readonly ItemSubType Ingots = new ItemSubType("Ingots", ItemCategoryOld.RawMaterial);
        static public readonly ItemSubType Ore = new ItemSubType("Ore", ItemCategoryOld.RawMaterial);
        static public readonly ItemSubType Rock = new ItemSubType("Rock", ItemCategoryOld.RawMaterial);
        static public readonly ItemSubType Logs = new ItemSubType("Logs", ItemCategoryOld.RawMaterial);
        static public readonly ItemSubType Planks = new ItemSubType("Planks", ItemCategoryOld.RawMaterial);
        static public readonly ItemSubType Bags = new ItemSubType("Bags", ItemCategoryOld.RawMaterial);

        static public readonly ItemSubType Sword = new ItemSubType("Sword", ItemCategoryOld.Weapon);

        static public readonly ItemSubType Axe = new ItemSubType("Axe", ItemCategoryOld.Tool);
        static public readonly ItemSubType Pickaxe = new ItemSubType("Pickaxe", ItemCategoryOld.Tool);
        static public readonly ItemSubType Hammer = new ItemSubType("Hammer", ItemCategoryOld.Tool);
        static public readonly ItemSubType Shovel = new ItemSubType("Shovel", ItemCategoryOld.Tool);
        static public readonly ItemSubType Hoe = new ItemSubType("Hoe", ItemCategoryOld.Tool);
        static public readonly ItemSubType Handsaw = new ItemSubType("Handsaw", ItemCategoryOld.Tool);

        static public readonly ItemSubType Workbench = new ItemSubType("Workbench", ItemCategoryOld.Facility);
        static public readonly ItemSubType Smeltery = new ItemSubType("Smeltery", ItemCategoryOld.Facility);

        static public readonly ItemSubType Human = new ItemSubType("Human", ItemCategoryOld.Entity);

        static public readonly ItemSubType Sapling = new ItemSubType("Sapling", ItemCategoryOld.Vegetation);

        static public readonly ItemSubType Stool = new ItemSubType("Stool", ItemCategoryOld.Furniture);
        static public readonly ItemSubType Chair = new ItemSubType("Chair", ItemCategoryOld.Furniture);


    }
}
