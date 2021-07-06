namespace Start_a_Town_.Components.Crafting
{
    partial class Reaction
    {
        static public readonly Reaction Sword = new Reaction(
            "Sword",
            //CanBeMadeAt(Site.Person, ItemTemplate.Workbench.ID),
            CanBeMadeAt(IsWorkstation.Types.None, IsWorkstation.Types.Workbench),//Block.Workbench),
            Reagent.Create(
                new Reagent("Hilt", Reagent.CanProduce(Product.Types.Tools)),
                new Reagent("Blade", Reagent.CanProduce(Product.Types.Tools))),
            //Product.Create(new Product(new ItemTemplate.Sword.Factory("Hilt", "Blade").Create))
            Product.Create(new Product(dic => ItemFactory.CreateTool(ToolDefs.Shovel, dic)))
            , SkillDef.Crafting
            );



        // MOVED IT TO PLANKS CLASS // EDIT: APPARENTLY NOT
        //static public readonly Reaction Planks = new(
        //    "Make Planks",
        //    //CanBeMadeAt(Site.Person, ItemTemplate.Workbench.ID),
        //    CanBeMadeAt(IsWorkstation.Types.Carpentry),//Block.Workbench),
        //    new() {
        //        //Reagent.Create(
        //        //"Base", RawMaterialDef.Logs, null, null, 1
        //        //new Reagent("Base", Reagent.IsOfSubType(ItemSubType.Logs))
        //        new Reagent("Base", new Ingredient(RawMaterialDef.Logs)
        //        ) },
        //    //Product.Create(new Product(new MaterialType.RawMaterial.Factory("Base").Create, new Quantity(4)))
        //    //Product.Create(new Product(dic=>ItemFactory.Craft(RawMaterialDef.Planks, dic), new Quantity(4)))
        //    Product.Create(new Product(dic => ItemFactory.CreateFrom(RawMaterialDef.Planks, dic["Base"].PrimaryMaterial), new Quantity(4)))
        //    , SkillDef.Carpentry
        //    //) { Tool = new ToolRequirement(Skills.Skill.Carpentry, true) };//{ Skill = Skills.Skill.Carpentry };
        //    );// { Skill = new ToolRequirement(Skills.Skill.Carpentry, true) };

        static public readonly Reaction Repairing = new Reaction("Repair", SkillDef.Tinkering)
            .AddBuildSite(IsWorkstation.Types.Workbench)
            .AddIngredient(new Ingredient("item")
                .SetAllow(ItemCategory.Tools, true)
                .AddResourceFilter(ResourceDef.Durability)
                .Preserve())
            .AddProduct(new Product("item").RestoreDurability());
    }
}
