using System.Collections.Generic;
using System.Linq;
using Start_a_Town_.Components.Crafting;

namespace Start_a_Town_
{
    public class RawMaterialDef : ItemDef
    {
        public RawMaterialDef CanBeProcessedInto;
        public RawMaterialDef(string name) : base(name)
        {
            this.ItemClass = typeof(Entity);
        }
        static public readonly ItemDef Planks = new ItemDef("Plank")
        {
            BaseValue = 5,
            Description = "Processed logs",
            StackCapacity = 24,
            Weight = .1f,
            Category = ItemCategory.Manufactured,
            Body = new Bone(BoneDefOf.Item, ItemContent.PlanksGrayscale) { DrawMaterialColor = true },
            CanProcessInto = new List<Reaction.Product.Types>() { Reaction.Product.Types.Tools, Reaction.Product.Types.Blocks, Reaction.Product.Types.Workbenches },
            DefaultMaterialType = MaterialTypeDefOf.Wood,
            CraftingProperties = new CraftingProperties() { 
                IsBuildingMaterial = true, 
                IsCraftingMaterial = true
            }
        }.SetMadeFrom(MaterialTypeDefOf.Wood);

        static public readonly ItemDef Logs = new ItemDef("Logs")
        {
            BaseValue = 1,
            Description = "It came from a tree",
            StackCapacity = 6,
            Body = new Bone(BoneDefOf.Item, ItemContent.LogsGrayscale) { DrawMaterialColor = true },
            Category = ItemCategory.RawMaterials,
            CanProcessInto = new List<Reaction.Product.Types>() { Reaction.Product.Types.Tools, Reaction.Product.Types.Blocks, Reaction.Product.Types.Workbenches },
            CraftingProperties = new CraftingProperties() { IsBuildingMaterial = true },
            DefaultMaterialType = MaterialTypeDefOf.Wood
        }.SetMadeFrom(MaterialTypeDefOf.Wood);

        static public readonly ItemDef Bags = new ItemDef("Bag")
        {
            BaseValue = 1,
            Description = "A bag containing grainy material",
            StackCapacity = 10,
            Category = ItemCategory.RawMaterials,
            Body = new Bone(BoneDefOf.Item, ItemContent.BagsGrayscale) { DrawMaterialColor = true },
            CanProcessInto = new List<Reaction.Product.Types>() { Reaction.Product.Types.Blocks },
            DefaultMaterialType = MaterialTypeDefOf.Soil,
        }.SetMadeFrom(MaterialTypeDefOf.Soil);

        static public readonly ItemDef Ingots = new ItemDef("Ingot")
        {
            BaseValue = 5,
            Description = "Used for crafting of weapons, armor, and tools.",
            StackCapacity = 20,
            Category = ItemCategory.Manufactured,
            Body = new Bone(BoneDefOf.Item, ItemContent.BarsGrayscale) { DrawMaterialColor = true },
            CanProcessInto = new List<Reaction.Product.Types>() { Reaction.Product.Types.Tools },
            DefaultMaterialType = MaterialTypeDefOf.Metal,
            CraftingProperties = new CraftingProperties() { IsCraftingMaterial = true, IsBuildingMaterial = true },
        }.SetMadeFrom(MaterialTypeDefOf.Metal);

        static public readonly ItemDef Ore = new ItemDef("Ore")
        {
            BaseValue = 1,
            Description = "A piece of mineral ore",
            StackCapacity = 10,
            Category = ItemCategory.RawMaterials,
            Body = new Bone(BoneDefOf.Item, ItemContent.OreGrayscale) { DrawMaterialColor = true },
            CanProcessInto = new List<Reaction.Product.Types>() { Reaction.Product.Types.Tools, Reaction.Product.Types.Blocks },
            DefaultMaterialType = MaterialTypeDefOf.Metal,
        }.SetMadeFrom(MaterialToken.Metal);

        static public readonly ItemDef Boulders = new ItemDef("Boulders")
        {
            BaseValue = 1,
            Description = "Chunks of rock",
            StackCapacity = 10,
            Category = ItemCategory.RawMaterials,
            Body = new Bone(BoneDefOf.Item, ItemContent.OreGrayscale) { DrawMaterialColor = true },
            CanProcessInto = new List<Reaction.Product.Types>() { Reaction.Product.Types.Tools, Reaction.Product.Types.Blocks },
            DefaultMaterialType = MaterialTypeDefOf.Stone,
            CraftingProperties = new CraftingProperties() { IsBuildingMaterial = true, IsCraftingMaterial = true },
        }.SetMadeFrom(MaterialTypeDefOf.Stone);

        static public readonly ItemDef Scraps = new ItemDef("Scraps")
        {
            StackDimension = 4,
            StackCapacity = 50,
            BaseValue = 0,
            Description = "Worthless but can be repurposed",
            Category = ItemCategory.RawMaterials,
            Body = new Bone(BoneDefOf.Item, Sprite.Default) { DrawMaterialColor = true },
        }.SetMadeFrom(MaterialTypeDefOf.Wood, MaterialTypeDefOf.Stone, MaterialTypeDefOf.Metal);

        static public void Initialize()
        {
            var defs = Def.Database.Values.OfType<ItemDef>().ToList();
            foreach (var def in defs.Where(d => d.DefaultMaterialType != null))
            {
                foreach (var m in def.DefaultMaterialType.SubTypes)
                    GameObject.AddTemplate(CreateFrom(def, m));
            }

            foreach (var mt in new[] { MaterialTypeDefOf.Metal, MaterialTypeDefOf.Wood, MaterialTypeDefOf.Stone }) // TODO make scraps from solid rawmaterials
                foreach (var m in mt.SubTypes)
                    GameObject.AddTemplate(CreateFrom(Scraps, m));

            InitRecipes();
        }

        static void InitRecipes()
        {
            Reaction.Register(new Reaction(
                "Make Planks new", SkillDefOf.Carpentry, JobDefOf.Carpenter)
                .AddBuildSite(IsWorkstation.Types.Carpentry)
                .AddIngredient(new Ingredient("Base")
                    .SetAllow(MaterialTypeDefOf.Wood, true)
                    .SetAllow(Logs, true)
                    .SetAllow(Scraps, true))
                .AddProduct(new Reaction.Product(Planks).GetMaterialFromIngredient("Base")));

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

            var smelting = new Reaction("Smelt", SkillDefOf.Crafting, JobDefOf.Smelter)
                .AddBuildSite(IsWorkstation.Types.Smeltery)
                .AddIngredient(new Ingredient("a")
                    .SetAllow(RawMaterialDef.Scraps, true)
                    .SetAllow(RawMaterialDef.Ore, true)
                    .SetAllow(MaterialTypeDefOf.Metal, true))
                .AddProduct(new Reaction.Product(RawMaterialDef.Ingots).GetMaterialFromIngredient("a")
                );
            Reaction.Register(smelting);
        }
        static public Entity CreateFrom(ItemDef def, MaterialDef mat)
        {
            var item = ItemFactory.CreateFrom(def, mat);
            return item;
        }
    }
}
