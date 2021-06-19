using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.Animations;
using Start_a_Town_.Components.Crafting;

namespace Start_a_Town_
{
    public class RawMaterialDef : ItemDef
    {
        public RawMaterialDef CanBeProcessedInto;
        //public List<MaterialType> CanBeMadeFrom = new List<MaterialType>();
        public RawMaterialDef(string name) : base(name)
        {
            //this.Factory= ItemFactory.CreateRawMaterial
            this.ItemClass = typeof(Entity);
        }
        static public readonly ItemDef Planks = new ItemDef("Plank")
        {
            BaseValue = 5,
            Description = "Processed logs",
            SubType = ItemSubType.Planks,
            StackCapacity = 20,
            Weight = .1f,
            Category = ItemCategory.Manufactured,
            Body = new Bone(BoneDef.Item, ItemContent.PlanksGrayscale) { DrawMaterialColor = true },
            CanProcessInto = new List<Reaction.Product.Types>() { Reaction.Product.Types.Tools, Reaction.Product.Types.Blocks, Reaction.Product.Types.Workbenches },
            DefaultMaterialType = MaterialType.Wood,
            //MadeFrom = MaterialToken.Wood,
            CraftingProperties = new CraftingProperties() { 
                IsBuildingMaterial = true, 
                IsCraftingMaterial = true
                //,
                //Reagents = new() {
                //    { BoneDef.EquipmentHead, new Reaction.Reagent(HeadIngredientIndex, new Ingredient(null, null, null).IsCraftingMaterial().HasReactionClass(ReactionClass.Tools)) }
                //}
            }
        }.SetMadeFrom(MaterialType.Wood);
        static public readonly ItemDef Logs = new ItemDef("Logs")
        {
            BaseValue = 1,
            Description = "It came from a tree",
            SubType = ItemSubType.Logs,
            StackCapacity = 10,
            Body = new Bone(BoneDef.Item, ItemContent.LogsGrayscale) { DrawMaterialColor = true },
            //CanBeProcessedInto = Planks,
            Category = ItemCategory.RawMaterials,
            //CanBeMadeFrom = new List<MaterialType>() { MaterialType.Wood },
            CanProcessInto = new List<Reaction.Product.Types>() { Reaction.Product.Types.Tools, Reaction.Product.Types.Blocks, Reaction.Product.Types.Workbenches },
            CraftingProperties = new CraftingProperties() { IsBuildingMaterial = true },
            DefaultMaterialType = MaterialType.Wood
            //MadeFrom = MaterialToken.Wood
        }.SetMadeFrom(MaterialType.Wood);
        static public readonly ItemDef Bags = new ItemDef("Bag")
        {
            BaseValue = 1,
            Description = "A bag containing grainy material",
            SubType = ItemSubType.Bags,
            StackCapacity = 10,
            Category = ItemCategory.RawMaterials,
            Body = new Bone(BoneDef.Item, ItemContent.BagsGrayscale) { DrawMaterialColor = true },
            CanProcessInto = new List<Reaction.Product.Types>() { Reaction.Product.Types.Blocks },
            DefaultMaterialType = MaterialType.Soil,
        }.SetMadeFrom(MaterialType.Soil);
        static public readonly ItemDef Ingots = new ItemDef("Ingot")
        {
            BaseValue = 5,
            Description = "Used for crafting of weapons, armor, and tools.",
            SubType = ItemSubType.Ingots,
            StackCapacity = 20,
            Category = ItemCategory.Manufactured,
            Body = new Bone(BoneDef.Item, ItemContent.BarsGrayscale) { DrawMaterialColor = true },
            CanProcessInto = new List<Reaction.Product.Types>() { Reaction.Product.Types.Tools },
            DefaultMaterialType = MaterialType.Metal,
            //MadeFrom = MaterialToken.Metal,
            CraftingProperties = new CraftingProperties() { IsCraftingMaterial = true },
        }.SetMadeFrom(MaterialType.Metal);

        static public readonly ItemDef Ore = new ItemDef("Ore")
        {
            BaseValue = 1,
            Description = "A piece of mineral ore",
            SubType = ItemSubType.Ore,
            StackCapacity = 10,
            Category = ItemCategory.RawMaterials,
            Body = new Bone(BoneDef.Item, ItemContent.OreGrayscale) { DrawMaterialColor = true },
            //CanBeProcessedInto = Bars,
            CanProcessInto = new List<Reaction.Product.Types>() { Reaction.Product.Types.Tools, Reaction.Product.Types.Blocks },
            DefaultMaterialType = MaterialType.Metal,
            //MadeFrom = MaterialToken.Metal
        }.SetMadeFrom(MaterialToken.Metal);

        static public readonly ItemDef Boulders = new("Boulders")
        {
            BaseValue = 1,
            Description = "Chunks of rock",
            SubType = ItemSubType.Rock,
            StackCapacity = 10,
            Category = ItemCategory.RawMaterials,
            Body = new Bone(BoneDef.Item, ItemContent.OreGrayscale) { DrawMaterialColor = true },
            CanProcessInto = new List<Reaction.Product.Types>() { Reaction.Product.Types.Tools, Reaction.Product.Types.Blocks },
            DefaultMaterialType = MaterialType.Stone,
            CraftingProperties = new CraftingProperties() { IsBuildingMaterial = true, IsCraftingMaterial = true },
        };
        static public readonly ItemDef Scraps = new ItemDef("Scraps")
        {
            StackDimension = 4,
            StackCapacity = 20,
            BaseValue = 0,
            Description = "Worthless but can be repurposed",
            Category = ItemCategory.RawMaterials,
            Body = new Bone(BoneDef.Item, Sprite.Default) { DrawMaterialColor = true },
        }.SetMadeFrom(MaterialType.Wood, MaterialType.Stone, MaterialType.Metal);
        static public void Initialize()
        {
            var defs = Def.Database.Values.OfType<ItemDef>().ToList();
            foreach (var def in defs.Where(d => d.DefaultMaterialType != null))
            {
                foreach (var m in def.DefaultMaterialType.SubTypes)
                    GameObject.AddTemplate(CreateFrom(def, m));
            }

            foreach (var mt in new[] { MaterialType.Metal, MaterialType.Wood, MaterialType.Stone }) // TODO make scraps from solid rawmaterials
                foreach (var m in mt.SubTypes)
                    GameObject.AddTemplate(CreateFrom(Scraps, m));

            InitRecipes();
        }
        

        static void InitRecipes()
        {
            Reaction.Register(new Reaction(
                "Make Planks new", SkillDef.Carpentry)
                .AddBuildSite(IsWorkstation.Types.Carpentry)
                //.SetSkill(SkillDef.Carpentry)
                .AddIngredient(new Ingredient("Base")// new Reaction.Reagent("Base", new Ingredient()
                    .SetAllow(MaterialType.Wood, true)
                    .SetAllow(RawMaterialDef.Logs, true)
                    .SetAllow(RawMaterialDef.Scraps, true))
                //.AddProduct(new Reaction.Product(dic => ItemFactory.CreateFrom(RawMaterialDef.Planks, dic["Base"].DominantMaterial), new Quantity(4))));
                .AddProduct(new Reaction.Product(RawMaterialDef.Planks, 4).GetMaterialFromIngredient("Base")));

        }

        static RawMaterialDef()
        {
            Register(Logs);
            Register(Planks);
            Register(Bags);
            Register(Ingots);
            Register(Ore);
            Register(Boulders);
            Register(Scraps);
        }
        static public Entity CreateFrom(ItemDef def, Material mat)
        {
            var item = ItemFactory.CreateFrom(def, mat);
            //item.GetInfo().Prefix = mat.Name;
            //item.Name = mat.Name + " " + item.Name;
            //item.ID = MaterialType.RawMaterial.GetNextID();
            return item;
        }
    }
}
