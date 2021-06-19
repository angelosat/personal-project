using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.Components;
using Start_a_Town_.Components.Crafting;

namespace Start_a_Town_
{
    static class ItemDefOf
    {
        static public readonly ItemDef Seeds = new("Seeds")
        {
            //Factory = ItemFactory.CreateSeeds,
            ItemClass = typeof(Entity),
            StackCapacity = 16,
            Category = ItemCategory.RawMaterials,
            Body = new Bone(BoneDef.Item, ItemContent.SeedsFull),
            DefaultMaterial = MaterialDefOf.Seed,
            CompProps = new List<ComponentProps>() { new SeedComponent.Props() }
        };
        //static public readonly ItemDef Template_Fruit = new("Fruit")
        //{
        //    ItemClass = typeof(Entity),
        //    StackCapacity = 8,
        //    Category = ItemCategory.FoodRaw,
        //    Body = new Bone(BoneDef.Item, Sprite.Default),
        //};
        static public readonly ItemDef Fruit = new ItemDef("Fruit")
        {
            Label = "Fruit",
            ItemClass = typeof(Entity),
            StackCapacity = 8,
            Category = ItemCategory.FoodRaw,
            Body = new Bone(BoneDef.Item, ItemContent.BerriesFull),
            ConsumableProperties = new ConsumableProperties()
            {
                //Byproduct = consumedItem => ItemFactory.CreateSeeds(PlantDefOf.Bush).SetStackSize(2) as Entity,
                FoodClasses = new[] { FoodClass.Fruit }
            },
            CompProps = new List<ComponentProps>()
            {
                new ConsumableComponent.Props(new[] { new NeedEffect(NeedDef.Hunger, 50) })
            },
            //CraftingProperties = new()
            //{ 
            //    ReactionClass = ReactionClass.Fruit
            //}
        }.SetMadeFrom(MaterialType.Fruit);
        static public readonly ItemDef Meat = new ItemDef("Meat")
        {
            ItemClass = typeof(Entity),
            StackCapacity = 8,
            Category = ItemCategory.FoodRaw,
            Body = new Bone(BoneDef.Item, Sprite.Default),
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
            Body = new Bone(BoneDef.Item, Sprite.Default),
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
            Body = new Bone(BoneDef.Item, ItemContent.BarsGrayscale),
            Category = ItemCategory.RawMaterials,
            DefaultMaterial = MaterialDefOf.Gold,
            BaseValue = 1,
        };

        //.AddCompProp(new ConsumableComponent.Props(new NeedEffect(NeedDef.Hunger, 50)));

        //static ItemDefs()
        //{
        //    GameObject.AddTemplate(ItemFactory.CreateSeeds(PlantDefOf.BerryBush));
        //}

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

            Reaction.Register(new Reaction("Extract Seeds", SkillDef.Argiculture)
                .AddBuildSite(IsWorkstation.Types.PlantProcessing)
                .AddIngredient("a", new Ingredient()
                    .SetAllow(ItemDefOf.Fruit, true))
                .AddProduct(new Reaction.Product(ItemDefOf.Seeds, 4)
                    .GetMaterialFromIngredient("a"))
                ); 

            //var ExtractSeeds = new Reaction(
            //            "Extract Seeds",
            //            CanBeMadeAt(IsWorkstation.Types.PlantProcessing),
            //            Reagent.Create(new Reaction.Reagent("Body", Reagent.HasSeeds())),
            //            Product.Create(new Product(mats => GameObject.Objects[GameObject.Types.Seeds].Clone() as Entity, 4)),
            //            SkillDef.Argiculture,
            //            JobDefOf.Cook);
        }

        private static void GenerateCookingRecipes()
        {
            var cookables = Def.GetDefs<ItemDef>().Where(d => d.RecipeProperties != null);
            foreach (var def in cookables)
                Reaction.AddRecipe(def.CreateRecipe());
            //foreach(var def in cookables)
            //{
            //    var recipeProps = def.RecipeProperties;
            //    Reaction.AddRecipe(new Reaction($"{recipeProps.Verb} {def.Label}", JobDef.Cook, new[] { IsWorkstation.Types.Baking })
            //        .AddIngredient(
            //            new Ingredient(recipeProps.IngredientName)
            //            .SetAllow(def.MadeFrom, true)
            //            .SetAllow(recipeProps.IngredientCategory, true))
            //        .AddProduct(
            //            new Reaction.Product(def)
            //            .GetMaterialFromIngredient(recipeProps.IngredientName)
            //        ));
            //}

                //Reaction.AddRecipe(new Reaction("Bake Pie", JobDef.Cook, new[] { IsWorkstation.Types.Baking })
                //    .AddIngredient(
                //        new Ingredient("filling")
                //        .SetAllow(Pie.MadeFrom, true)
                //        .SetAllow(ItemCategory.FoodRaw, true))
                //    .AddProduct(
                //        new Reaction.Product(ItemDefOf.Pie)
                //        .GetMaterialFromIngredient("filling")
                //    ));
        }
    }
}
