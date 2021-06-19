using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.Components.Materials;
using Start_a_Town_.Components.Items;
using Start_a_Town_.Tokens;

namespace Start_a_Town_.Components.Crafting
{
    partial class Reaction
    {
        static public readonly Reaction Pickaxe = new Reaction(
            "Pickaxe",
            //CanBeMadeAt(Site.Person, ItemTemplate.Workbench.ID),
            CanBeMadeAt(IsWorkstation.Types.None, IsWorkstation.Types.Workbench),
            Reagent.Create(
                new Reagent("Handle", Reagent.CanProduce(Product.Types.Tools)),
                new Reagent("Head", Reagent.CanProduce(Product.Types.Tools))),
            //Product.Create(new Product(ItemTemplates.Templates.Pickaxe, new GetPartMaterialFromReagent("Handle", "Handle"), new GetPartMaterialFromReagent("Head", "Head")))
            Product.Create(new Product(new ItemTemplate.Pickaxe.Factory("Handle", "Head").Create))//, new GetPartMaterialFromReagent("Handle", "Handle"), new GetPartMaterialFromReagent("Head", "Head")))
            );

        static public readonly Reaction Shovel = new Reaction(
            "Shovel",
            //CanBeMadeAt(Site.Person, ItemTemplate.Workbench.ID),
            CanBeMadeAt(IsWorkstation.Types.None, IsWorkstation.Types.Workbench),
            Reagent.Create(
                new Reagent("Handle", Reagent.CanProduce(Product.Types.Tools)),
                new Reagent("Head", Reagent.CanProduce(Product.Types.Tools))),
            Product.Create(new Product(new ItemTemplate.Shovel.Factory("Handle", "Head").Create))
            );

        static public readonly Reaction Sword = new Reaction(
            "Sword",
            //CanBeMadeAt(Site.Person, ItemTemplate.Workbench.ID),
            CanBeMadeAt(IsWorkstation.Types.None, IsWorkstation.Types.Workbench),//Block.Workbench),
            Reagent.Create(
                new Reagent("Hilt", Reagent.CanProduce(Product.Types.Tools)),
                new Reagent("Blade", Reagent.CanProduce(Product.Types.Tools))),
            Product.Create(new Product(new ItemTemplate.Sword.Factory("Hilt", "Blade").Create))
            );

        static public readonly Reaction Axe = new Reaction(
            "Axe",
            //CanBeMadeAt(Site.Person, ItemTemplate.Workbench.ID),
            CanBeMadeAt(IsWorkstation.Types.None, IsWorkstation.Types.Workbench),//Block.Workbench),
            Reagent.Create(
                new Reagent("Handle", Reagent.CanProduce(Product.Types.Tools)),
                new Reagent("Head", Reagent.CanProduce(Product.Types.Tools))),
            Product.Create(new Product(new ItemTemplate.Axe.Factory("Handle", "Head").Create))
            );

        static public readonly Reaction Hammer = new Reaction(
            "Hammer",
            //CanBeMadeAt(Site.Person, ItemTemplate.Workbench.ID), //GameObject.Types.BenchReactions,
            CanBeMadeAt(IsWorkstation.Types.None, IsWorkstation.Types.Workbench),//Block.Workbench),
            Reagent.Create(
                new Reagent("Handle", Reagent.CanProduce(Product.Types.Tools)),
                new Reagent("Head", Reagent.IsOfSubType(ItemSubType.Bars), Reagent.HasHardness(0.4f, 1f), Reagent.CanProduce(Product.Types.Tools))),
            Product.Create(
                new Product(new ItemTemplate.Hammer.Factory("Handle", "Head").Create,
                    new GetDurabilityFromTool()))
            ) { Skill = new ToolRequirement(Skills.Skill.Carpentry, false) };

        static public readonly Reaction Hoe = new Reaction(
            "Hoe",
            //CanBeMadeAt(Site.Person, ItemTemplate.Workbench.ID),
            CanBeMadeAt(IsWorkstation.Types.None, IsWorkstation.Types.Workbench),//Block.Workbench),
            Reagent.Create(
                new Reagent("Handle", Reagent.CanProduce(Product.Types.Tools)),
                new Reagent("Head", Reagent.CanProduce(Product.Types.Tools))),
            Product.Create(new Product(new ItemTemplate.Hoe.Factory("Handle", "Head").Create))
            );

        static public readonly Reaction Handsaw = new Reaction(
            "Handsaw",
            //CanBeMadeAt(Site.Person, ItemTemplate.Workbench.ID),
            CanBeMadeAt(IsWorkstation.Types.None, IsWorkstation.Types.Workbench),//Block.Workbench),
            Reagent.Create(
                new Reagent("Handle", Reagent.CanProduce(Product.Types.Tools)),
                new Reagent("Blade", Reagent.CanProduce(Product.Types.Tools))),
            Product.Create(new Product(new ItemTemplate.Handsaw.Factory("Handle", "Blade"), new FlimsyIfNoBench()))
            );

        // MOVED IT TO PLANKS CLASS // EDIT: APPARENTLY NOT
        static public readonly Reaction Planks = new Reaction(
            "Planks",
            //CanBeMadeAt(Site.Person, ItemTemplate.Workbench.ID),
            CanBeMadeAt(IsWorkstation.Types.Carpentry),//Block.Workbench),
            Reagent.Create(
                new Reagent("Base", Reagent.IsOfSubType(ItemSubType.Logs))),
            Product.Create(new Product(new MaterialType.RawMaterial.Factory("Base").Create, new Quantity(4)))
            //) { Tool = new ToolRequirement(Skills.Skill.Carpentry, true) };//{ Skill = Skills.Skill.Carpentry };
            );// { Skill = new ToolRequirement(Skills.Skill.Carpentry, true) };

        //static public readonly Reaction CreateFurnitureParts = new Reaction(
        //    "Furniture Parts",
        //    //CanBeMadeAt(Site.Person, ItemTemplate.Workbench.ID),
        //    CanBeMadeAt(IsWorkstation.Types.Carpentry),//Block.Workbench),
        //    Reagent.Create(
        //        new Reagent("Material", Reagent.IsOfSubType(ItemSubType.Planks))),
        //    Product.Create(new Product(new FurnitureParts("Material"), new Quantity(4)))
        //    //) { Tool = new ToolRequirement(Skills.Skill.Carpentry, true) };//{ Skill = Skills.Skill.Carpentry };
        //    );// { Skill = new ToolRequirement(Skills.Skill.Carpentry, true) };


        

        static public readonly Reaction Workbench = new Reaction(
            "Workbench",
            //CanBeMadeAt(Site.Person, ItemTemplate.Workbench.ID),
            CanBeMadeAt(IsWorkstation.Types.None, IsWorkstation.Types.Workbench),
            Reagent.Create(new Reaction.Reagent("Body", Reagent.IsOfMaterialType(MaterialType.Wood), Reagent.CanProduce(Product.Types.Workbenches))),
            Product.Create(new Product(new ItemTemplate.Workbench.Factory().Create)) // maybe not create a new factory?
            );

        static public readonly Reaction Smelting = new Reaction(
            "Smelting",
            //CanBeMadeAt(Site.Person, ItemTemplate.Workbench.ID),
            CanBeMadeAt(IsWorkstation.Types.Smeltery),
            Reagent.Create(
                new Reaction.Reagent("Ore", Reagent.IsOfSubType(ItemSubType.Ore)),
                new Reaction.Reagent("Fuel", new IsFuel())
                ),

            //Product.Create(new Product(MaterialType.Bars))
            Product.Create(new Product(new MaterialType.RawMaterial.Factory("Ore").Create))
            //Product.Create(new Product(mats =>
            //{
            //    var mat = mats.First().Object;
            //    var chain = mat.GetComponent<MaterialsComponent>().Parts["Body"].Material.ProcessingChain;
            //    var currentStep = chain.FindIndex(item => item.ID == mat.ID);
            //    //if (currentStep == chain.Count - 1)
            //    //    return;
            //    var template = chain[currentStep + 1];
            //    var product = template.Clone();
            //    return product;
            //}))
            );

        static public readonly Reaction Burning = new Reaction(
            "Burn",
            //CanBeMadeAt(Site.Person, ItemTemplate.Workbench.ID),
            CanBeMadeAt(IsWorkstation.Types.Smeltery),
            Reagent.Create(new Reaction.Reagent("Body", Reagent.IsOfMaterialType(MaterialType.Wood))),
            //Product.Create(new Product(MaterialType.Bars))
            Product.Create(new Product(mats =>
            {
                //return MaterialType.Rocks.CreateFrom(Material.Coal);
                return MaterialType.RawMaterial.Create(Material.Coal);

            }))
            );

        /*
    //static public readonly Reaction WoodenDeck = new Reaction(
    //    "WoodenDeck",
    //    GameObject.Types.BenchReactions,
    //    Reagent.Create(new Reagent("Base", Reagent.IsOfMaterialType(MaterialType.Wood), Reagent.CanProduce(Product.Types.Blocks))),
    //    Product.Create(new Product(GameObject.Types.WoodenDeck))
    //    );

    //static public readonly Reaction Cobblestone = new Reaction(
    //    "Cobblestone",
    //    GameObject.Types.BenchReactions,
    //    Reagent.Create(new Reagent("Base", Reagent.IsOfMaterial(Material.Stone), Reagent.CanProduce(Product.Types.Blocks))),
    //    Product.Create(new Product(GameObject.Types.CobblestoneItem))
    //    );
    
     * */


        //static public readonly Reaction Workbench = new Reaction(
        //    "Workbench",
        //    GameObject.Types.BenchReactions,
        //    Reagent.Create(new Reagent("Base", Reagent.CanProduce(Product.Types.Workbenches))),
        //    Product.Create(new Product(GameObject.Types.BenchReactions))
        //    );

        //static public readonly Reaction Sword = new Reaction(
        //    "Sword",
        //    GameObject.Types.BenchReactions,
        //    Reagent.Create(new Reagent("Base", Reagent.CanProduce(Product.Types.Tools))),
        //    Product.Create(new Product(GameObject.Types.Sword, new GetMaterialFromReagent("Base")))
        //    );

        //static public readonly Reaction Axe = new Reaction(
        //    "Axe",
        //    GameObject.Types.BenchReactions,
        //    Reagent.Create(new Reagent("Base", Reagent.CanProduce(Product.Types.Tools))),
        //    Product.Create(new Product(GameObject.Types.Axe, new GetMaterialFromReagent("Base")))
        //    );

        //static public readonly Reaction Hoe = new Reaction(
        //    "Hoe",
        //    GameObject.Types.BenchReactions,
        //    Reagent.Create(new Reagent("Base", Reagent.CanProduce(Product.Types.Tools))),
        //    Product.Create(new Product(GameObject.Types.Hoe, new GetMaterialFromReagent("Base")))
        //    );

        //static public readonly Reaction Shovel = new Reaction(
        //    "Shovel",
        //    GameObject.Types.BenchReactions,
        //    Reagent.Create(new Reagent("Base", Reagent.CanProduce(Product.Types.Tools))),
        //    Product.Create(new Product(GameObject.Types.Shovel, new GetMaterialFromReagent("Base")))
        //    );

        //static public readonly Reaction Hammer = new Reaction(
        //    "Hammer",
        //    GameObject.Types.BenchReactions,
        //    Reagent.Create(new Reagent("Base", Reagent.CanProduce(Product.Types.Tools))),
        //    Product.Create(new Product(GameObject.Types.Hammer, new GetMaterialFromReagent("Base")))
        //    );

        //static public readonly Reaction Handsaw = new Reaction(
        //    "Handsaw",
        //    GameObject.Types.BenchReactions,
        //    Reagent.Create(new Reagent("Base", Reagent.CanProduce(Product.Types.Tools))),
        //    Product.Create(new Product(GameObject.Types.Handsaw, new GetMaterialFromReagent("Base")))
        //    );

        //static public readonly Reaction Helmet = new Reaction(
        //    "Helmet",
        //    GameObject.Types.BenchReactions,
        //    Reagent.Create(new Reagent("Base", Reagent.CanProduce(Product.Types.Tools))),
        //    Product.Create(new Product(GameObject.Types.Helmet, new GetMaterialFromReagent("Base")))
        //    );
    }
}
