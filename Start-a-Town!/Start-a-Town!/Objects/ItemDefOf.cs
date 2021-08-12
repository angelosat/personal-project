using System.Collections.Generic;
using System.Linq;
using Start_a_Town_.Components;
using Start_a_Town_.Components.Crafting;

namespace Start_a_Town_
{
    static class ItemDefOf
    {
        static public readonly ItemDef Seeds = new("Seeds")
        {
            ItemClass = typeof(Entity),
            StackCapacity = 16,
            Category = ItemCategory.RawMaterials,
            Body = new Bone(BoneDefOf.Item, ItemContent.SeedsFull),
            DefaultMaterial = MaterialDefOf.Seed,
            CompProps = new List<ComponentProps>() { new SeedComponent.Props() }
        };
        
        static public readonly ItemDef Fruit = new ItemDef("Fruit")
        {
            ItemClass = typeof(Entity),
            StackCapacity = 8,
            Category = ItemCategory.FoodRaw,
            Body = new Bone(BoneDefOf.Item, ItemContent.BerriesFull),
            ConsumableProperties = new ConsumableProperties()
            {
                FoodClasses = new[] { FoodClass.Fruit }
            },
            CompProps = new List<ComponentProps>()
            {
                new ConsumableComponent.Props(new[] { new NeedEffect(NeedDef.Hunger, 50) })
            },
        }.SetMadeFrom(MaterialType.Fruit);

        static public readonly ItemDef Meat = new ItemDef("Meat")
        {
            ItemClass = typeof(Entity),
            StackCapacity = 8,
            Category = ItemCategory.FoodRaw,
            Body = new Bone(BoneDefOf.Item, Sprite.Default),
            DefaultMaterialType = MaterialType.Meat,
            ConsumableProperties = new ConsumableProperties(),
            CompProps = new List<ComponentProps>()
            {
                new ConsumableComponent.Props(new[] { new NeedEffect(NeedDef.Hunger, 50) })
            },
        }.SetMadeFrom(MaterialType.Meat);

        static public readonly ItemDef Pie = new ItemDef("Pie")
        {
            ItemClass = typeof(Entity),
            StackCapacity = 4,
            Category = ItemCategory.FoodCooked,
            Body = new Bone(BoneDefOf.Item, Sprite.Default),
            ConsumableProperties = new()
            {
                FoodClasses = new[] { FoodClass.Dish }
            },
            CraftingProperties = new CraftingProperties().MakeableFrom(ItemCategory.FoodRaw),
            RecipeProperties =
                new RecipeProperties("Bake") { Job = JobDefOf.Cook }
                    .AddWorkstation(IsWorkstation.Types.Baking)
                    .AddIngredientMaker(def =>
                        new Ingredient("Filling") { DefaultRestrictions = new IngredientRestrictions().Restrict(MaterialType.Meat) }
                            .SetAllow(def.ValidMaterialTypes, true)
                            .SetAllow(ItemCategory.FoodRaw, true))
                    .AddProductMaker(def => new Reaction.Product(def).GetMaterialFromIngredient("Filling")),
            CompProps = new List<ComponentProps>()
            {
                new ConsumableComponent.Props()
            },
        }.SetMadeFrom(MaterialType.Fruit, MaterialType.Meat);

        static public readonly ItemDef Coins = new("Coins")
        {
            ItemClass = typeof(Entity),
            StackCapacity = ushort.MaxValue,
            Body = new Bone(BoneDefOf.Item, ItemContent.BarsGrayscale),
            Category = ItemCategory.RawMaterials,
            DefaultMaterial = MaterialDefOf.Gold,
            BaseValue = 1,
        };

        static ItemDefOf()
        {
            Def.Register(Meat);
            Def.Register(Fruit);
            Def.Register(Seeds);
            Def.Register(Coins);
            Def.Register(Pie);

            GameObject.AddTemplate(ItemFactory.CreateItem(ItemDefOf.Coins).SetStackSize(100));
            GameObject.AddTemplates(Fruit.CreateFromAllMAterials());
            GameObject.AddTemplates(Meat.CreateFromAllMAterials());
            GameObject.AddTemplates(Pie.CreateFromAllMAterials());

            GenerateCookingRecipes();

            Reaction.Register(new Reaction("Extract Seeds", SkillDefOf.Argiculture)
                .AddBuildSite(IsWorkstation.Types.PlantProcessing)
                .AddIngredient("a", new Ingredient()
                    .SetAllow(ItemDefOf.Fruit, true))
                .AddProduct(new Reaction.Product(ItemDefOf.Seeds, 4)
                    .GetMaterialFromIngredient("a"))
                ); 
        }

        private static void GenerateCookingRecipes()
        {
            var cookables = Def.GetDefs<ItemDef>().Where(d => d.RecipeProperties != null);
            foreach (var def in cookables)
                Reaction.AddRecipe(def.CreateRecipe());
        }
    }
}
