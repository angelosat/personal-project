using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_.Components.Items
{
    public class ItemCategory
    {
        public string Name { get; set; }
        public ItemCategory(string name)
        {
            this.Name = name;
        }

        static public readonly ItemCategory Generic = new ItemCategory("Generic");
        static public readonly ItemCategory RawMaterial = new ItemCategory("Raw Material");
        static public readonly ItemCategory Weapon = new ItemCategory("Weapon");
        static public readonly ItemCategory Tool = new ItemCategory("Tool");
        static public readonly ItemCategory Facility = new ItemCategory("Facility");
        static public readonly ItemCategory Entity = new ItemCategory("Entity");
        static public readonly ItemCategory Vegetation = new ItemCategory("Vegetation");
        static public readonly ItemCategory Furniture = new ItemCategory("Furniture");

    }
    public class ItemSubType
    {
        public string Name { get; set; }
        public ItemCategory ItemType { get; set; }
        public ItemSubType(string name, ItemCategory type)
        {
            this.Name = name;
            this.ItemType = type;
        }

        public override string ToString()
        {
            return ((this.ItemType == ItemCategory.Generic) ? "" : this.ItemType.Name + ": ") + this.Name;
        }

        static public readonly ItemSubType Generic = new ItemSubType("Generic", ItemCategory.Generic); 

        static public readonly ItemSubType Bars = new ItemSubType("Bars", ItemCategory.RawMaterial);
        static public readonly ItemSubType Ore = new ItemSubType("Ore", ItemCategory.RawMaterial);
        static public readonly ItemSubType Rock = new ItemSubType("Rock", ItemCategory.RawMaterial);
        static public readonly ItemSubType Logs = new ItemSubType("Logs", ItemCategory.RawMaterial);
        static public readonly ItemSubType Planks = new ItemSubType("Planks", ItemCategory.RawMaterial);
        static public readonly ItemSubType Bags = new ItemSubType("Bags", ItemCategory.RawMaterial);

        static public readonly ItemSubType Sword = new ItemSubType("Sword", ItemCategory.Weapon);

        static public readonly ItemSubType Axe = new ItemSubType("Axe", ItemCategory.Tool);
        static public readonly ItemSubType Pickaxe = new ItemSubType("Pickaxe", ItemCategory.Tool);
        static public readonly ItemSubType Hammer = new ItemSubType("Hammer", ItemCategory.Tool);
        static public readonly ItemSubType Shovel = new ItemSubType("Shovel", ItemCategory.Tool);
        static public readonly ItemSubType Hoe = new ItemSubType("Hoe", ItemCategory.Tool);
        static public readonly ItemSubType Handsaw = new ItemSubType("Handsaw", ItemCategory.Tool);

        static public readonly ItemSubType Workbench = new ItemSubType("Workbench", ItemCategory.Facility);
        static public readonly ItemSubType Smeltery = new ItemSubType("Smeltery", ItemCategory.Facility);

        static public readonly ItemSubType Human = new ItemSubType("Human", ItemCategory.Entity);

        static public readonly ItemSubType Sapling = new ItemSubType("Sapling", ItemCategory.Vegetation);

        static public readonly ItemSubType Stool = new ItemSubType("Stool", ItemCategory.Furniture);
        static public readonly ItemSubType Chair = new ItemSubType("Chair", ItemCategory.Furniture);


    }
}
