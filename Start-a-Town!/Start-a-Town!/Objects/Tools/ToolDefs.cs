using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components.Crafting;

namespace Start_a_Town_
{
    static class ToolDefs
    {
        // TODO: make a single base tool def and create tools from it with different properties (graphics/usage)
        static readonly string HandleIngredientIndex = "Handle";
        static readonly string HeadIngredientIndex = "Head";

        public static readonly CraftingProperties ToolCraftingProperties = new()
        {
            Reagents = new Dictionary<BoneDef, Reaction.Reagent>()
                {
                    { BoneDefOf.ToolHandle, new Reaction.Reagent(HandleIngredientIndex, new Ingredient(null, null, null).SetAllow(ItemCategoryDefOf.Manufactured, true)) }, //.IsBuildingMaterial()
                    { BoneDefOf.ToolHead, new Reaction.Reagent(HeadIngredientIndex, new Ingredient(null, null, null).SetAllow(ItemCategoryDefOf.Manufactured, true))  }
                }
        };

        static public readonly ItemDef Tool = new("Tool")
        {
            ItemClass = typeof(Tool),
            QualityLevels = true,
            Category = ItemCategoryDefOf.Equipment,
            MadeFromMaterials = true,
            GearType = GearType.Mainhand,
            DefaultMaterial = MaterialDefOf.Iron,
            Factory = d => d.CreateNew(),
            CraftingProperties = ToolCraftingProperties,
            Body = new Bone(BoneDefOf.ToolHandle, ItemContent.LogsGrayscale, Vector2.Zero, 0.001f) { DrawMaterialColor = true, OriginGroundOffset = new Vector2(0, -16) }
                            .AddJoint(Vector2.Zero, new Bone(BoneDefOf.ToolHead, ItemContent.LogsGrayscale) { DrawMaterialColor = true }),
            NameGetter = e => e.ToolComponent.Props.Label,
            StorageFilterVariations = Def.GetDefs<ToolProps>(),
            VariationGetter = e => e.ToolComponent.Props
        };

        static public void Init() { }
        static ToolDefs()
        {
            Def.Register(Tool);

            ToolProps.Init();
        }
        
        private static void GenerateRecipes()
        {
            var defs = Def.Database.Values.OfType<ItemDef>();
            var craftableTools = defs.Where(d => d.CraftingProperties?.Reagents?.Any() ?? false);
            foreach (var tool in craftableTools)
            {
                var reagents = new List<Reaction.Reagent>();
               
                foreach (var reagent in tool.CraftingProperties.Reagents)
                    reagents.Add(reagent.Value);

                var reaction = new Reaction(
                    tool.Name,
                    Reaction.CanBeMadeAt(IsWorkstation.Types.None, IsWorkstation.Types.Workbench),
                    reagents,
                    new List<Reaction.Product>() { new Reaction.Product(dic => ItemFactory.CreateTool(tool, dic)) },
                    SkillDefOf.Crafting);
            }
        }
    }
}
 
