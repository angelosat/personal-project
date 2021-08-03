using System;

namespace Start_a_Town_.Components.Crafting
{
    partial class Reaction
    {
        [Obsolete]
        static public readonly Reaction Sword = new Reaction(
            "Sword",
            CanBeMadeAt(IsWorkstation.Types.None, IsWorkstation.Types.Workbench),
            Reagent.Create(
                new Reagent("Hilt", Reagent.CanProduce(Product.Types.Tools)),
                new Reagent("Blade", Reagent.CanProduce(Product.Types.Tools))),
            Product.Create(new Product(dic => ItemFactory.CreateTool(ToolDefs.Shovel, dic)))
            , SkillDef.Crafting
            );

        [Obsolete]
        static public readonly Reaction Repairing = new Reaction("Repair", SkillDef.Tinkering)
            .AddBuildSite(IsWorkstation.Types.Workbench)
            .AddIngredient(new Ingredient("item")
                .SetAllow(ItemCategory.Equipment, true)
                .AddResourceFilter(ResourceDef.Durability)
                .Preserve())
            .AddProduct(new Product("item").RestoreDurability());
    }
}
