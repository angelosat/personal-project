using System;

namespace Start_a_Town_.Components.Crafting
{
    partial class Reaction
    {
        [Obsolete]
        static public readonly Reaction Repairing = new Reaction("Repair", SkillDef.Tinkering)
            .AddBuildSite(IsWorkstation.Types.Workbench)
            .AddIngredient(new Ingredient("item")
                .SetAllow(ItemCategory.Equipment, true)
                .AddResourceFilter(ResourceDefOf.Durability)
                .Preserve())
            .AddProduct(new Product("item").RestoreDurability());
    }
}
