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
                    { BoneDef.EquipmentHandle, new Reaction.Reagent(HandleIngredientIndex, new Ingredient(null, null, null).SetAllow(ItemCategory.Manufactured, true)) }, //.IsBuildingMaterial()
                    { BoneDef.EquipmentHead, new Reaction.Reagent(HeadIngredientIndex, new Ingredient(null, null, null).SetAllow(ItemCategory.Manufactured, true))  }
                }
        };

        static public readonly ItemDef Tool = new("Tool")
        {
            ItemClass = typeof(Tool),
            QualityLevels = true,
            Category = ItemCategory.Tools,
            MadeFromMaterials = true,
            GearType = GearType.Mainhand,
            DefaultMaterial = MaterialDefOf.Iron,
            Factory = d => d.CreateNew(),
            CraftingProperties = ToolCraftingProperties,
            Body = new Bone(BoneDef.EquipmentHandle, ItemContent.LogsGrayscale, Vector2.Zero, 0.001f) { DrawMaterialColor = true, OriginGroundOffset = new Vector2(0, -16) }
                            .AddJoint(Vector2.Zero, new Bone(BoneDef.EquipmentHead, ItemContent.LogsGrayscale) { DrawMaterialColor = true }),
            Variator = Def.GetDefs<ToolProps>()
        };

        static public readonly ItemDef Shovel = new("Shovel")
        {
            BaseValue = 10,
            QualityLevels = true,
            Category = ItemCategory.Tools,
            Description = "Used to dig out grainy material like soil dirt and sand.",
            DefaultSprite = ItemContent.ShovelFull,
            MadeFromMaterials = true,
            GearType = GearType.Mainhand,
            ToolProperties = new ItemToolDef(new ToolAbility(ToolAbilityDef.Digging, 5)).AssociateJob(JobDefOf.Digger),
            DefaultMaterial = MaterialDefOf.Iron,
            Factory = ItemFactory.CreateTool,
            Randomizer = ItemFactory.CreateToolFromRandomMaterials,
            Body = new Bone(BoneDef.EquipmentHandle, ItemContent.ShovelHandle, Vector2.Zero, 0.001f) { DrawMaterialColor = true, OriginGroundOffset = new Vector2(0, -16) }
                            .AddJoint(Vector2.Zero, new Bone(BoneDef.EquipmentHead, ItemContent.ShovelHead) { DrawMaterialColor = true }),
            CraftingProperties = ToolCraftingProperties,
            CompProps = new List<ComponentProps>()
            { 
                new ComponentProps() { CompType = typeof(OwnershipComponent) },
                new ResourcesComponent.Props(ResourceDef.Durability)
            }
        };
        
        static public readonly ItemDef Axe = new("Axe")
        {
            Category = ItemCategory.Tools,
            Description = "Chops down trees.",
            QualityLevels = true,
            DefaultSprite = ItemContent.AxeFull,
            MadeFromMaterials = true,
            DefaultMaterial = MaterialDefOf.Iron,
            ToolProperties = new ItemToolDef(new ToolAbility(ToolAbilityDef.Chopping, 5)).AssociateJob(JobDefOf.Lumberjack),
            Factory = ItemFactory.CreateTool,
            Randomizer = ItemFactory.CreateToolFromRandomMaterials,
            GearType = GearType.Mainhand,
            BaseValue = 10,

            Body = new Bone(BoneDef.EquipmentHandle, ItemContent.AxeHandle, Vector2.Zero, 0.001f) { DrawMaterialColor = true, OriginGroundOffset = new Vector2(0, -16) }
                    .AddJoint(Vector2.Zero, new Bone(BoneDef.EquipmentHead, ItemContent.AxeHead) { DrawMaterialColor = true }),
            CraftingProperties = ToolCraftingProperties,
            CompProps = new List<ComponentProps>()
            {
                new ResourcesComponent.Props(ResourceDef.Durability)
            }
        };

        static public readonly ItemDef Hammer = new("Hammer")
        {
            Category = ItemCategory.Tools,
            Description = "Chops down trees.",
            BaseValue = 10,
            QualityLevels = true,
            DefaultSprite = ItemContent.HammerFull,
            GearType = GearType.Mainhand,
            MadeFromMaterials = true,
            DefaultMaterial = MaterialDefOf.Iron,
            ToolProperties = new ItemToolDef(new ToolAbility(ToolAbilityDef.Building, 5)).AssociateJob(JobDefOf.Builder),
            Factory = ItemFactory.CreateTool,
            Randomizer = ItemFactory.CreateToolFromRandomMaterials,
            Body = new Bone(BoneDef.EquipmentHandle, ItemContent.HammerHandle, Vector2.Zero, 0.001f) { DrawMaterialColor = true, OriginGroundOffset = new Vector2(0, -16) }
                    .AddJoint(Vector2.Zero, new Bone(BoneDef.EquipmentHead, ItemContent.HammerHead) { DrawMaterialColor = true }),
            CraftingProperties = ToolCraftingProperties,
            CompProps = new List<ComponentProps>()
            {
                new ResourcesComponent.Props(ResourceDef.Durability)
            }
        };

        static public readonly ItemDef Pickaxe = new("Pickaxe")
        {
            Category = ItemCategory.Tools,
            Description = "Used for mining.",
            MadeFromMaterials = true,
            GearType = GearType.Mainhand,
            QualityLevels = true,
            DefaultSprite = ItemContent.PickaxeFull,
            DefaultMaterial = MaterialDefOf.Iron,
            ToolProperties = new ItemToolDef(new ToolAbility(ToolAbilityDef.Mining, 5)).AssociateJob(JobDefOf.Miner),
            Factory = ItemFactory.CreateTool,
            Randomizer = ItemFactory.CreateToolFromRandomMaterials,
            BaseValue = 10,
            Body = new Bone(BoneDef.EquipmentHandle, ItemContent.PickaxeHandle, Vector2.Zero, 0.001f) { DrawMaterialColor = true, OriginGroundOffset = new Vector2(0, -16) }
                    .AddJoint(Vector2.Zero, new Bone(BoneDef.EquipmentHead, ItemContent.PickaxeHead) { DrawMaterialColor = true }),
            CraftingProperties = ToolCraftingProperties,
            CompProps = new List<ComponentProps>()
            {
                new ResourcesComponent.Props(ResourceDef.Durability)
            }
        };

        static public readonly ItemDef Handsaw = new("Handsaw")
        {
            BaseValue = 10,
            Category = ItemCategory.Tools,
            Description = "Converts logs to planks.",
            MadeFromMaterials = true,
            GearType = GearType.Mainhand,
            QualityLevels = true,
            DefaultSprite = ItemContent.HandsawFull,
            DefaultMaterial = MaterialDefOf.Iron,
            ToolProperties = new ItemToolDef(new ToolAbility(ToolAbilityDef.Carpentry, 5)).AssociateJob(JobDefOf.Carpenter),
            Factory = ItemFactory.CreateTool,
            Randomizer = ItemFactory.CreateToolFromRandomMaterials,
            Body = new Bone(BoneDef.EquipmentHandle, ItemContent.HandsawHandle, Vector2.Zero, 0.001f) { DrawMaterialColor = true, OriginGroundOffset = new Vector2(0, -16) }
                    .AddJoint(Vector2.Zero, new Bone(BoneDef.EquipmentHead, ItemContent.HandsawHead) { DrawMaterialColor = true }),
            CraftingProperties = ToolCraftingProperties,
            CompProps = new List<ComponentProps>()
            {
                new ResourcesComponent.Props(ResourceDef.Durability)
            }

        };
        static public readonly ItemDef Hoe = new("Hoe")
        {
            BaseValue = 10,
            Category = ItemCategory.Tools,
            Description = "Converts logs to planks.",
            MadeFromMaterials = true,
            QualityLevels = true,
            GearType = GearType.Mainhand,
            DefaultSprite = ItemContent.HoeFull,
            DefaultMaterial = MaterialDefOf.Iron,
            ToolProperties = new ItemToolDef(new ToolAbility(ToolAbilityDef.Argiculture, 5)).AssociateJob(JobDefOf.Farmer),
            Factory = ItemFactory.CreateTool,
            Randomizer = ItemFactory.CreateToolFromRandomMaterials,
            Body = new Bone(BoneDef.EquipmentHandle, ItemContent.HandsawHandle, Vector2.Zero, 0.001f) { DrawMaterialColor = true, OriginGroundOffset = new Vector2(0, -16) }
                    .AddJoint(Vector2.Zero, new Bone(BoneDef.EquipmentHead, ItemContent.HandsawHead) { DrawMaterialColor = true }),
            CraftingProperties = ToolCraftingProperties,
            CompProps = new List<ComponentProps>()
            {
                new ResourcesComponent.Props(ResourceDef.Durability)
            }

        };
        static public void Init() { }
        static ToolDefs()
        {
            Def.Register(Tool);

            Def.Register(Shovel);
            Def.Register(Axe);
            Def.Register(Hammer);
            Def.Register(Pickaxe);
            Def.Register(Handsaw);
            Def.Register(Hoe);

            GameObject.AddTemplate(ItemFactory.CreateTool(Shovel));
            GameObject.AddTemplate(ItemFactory.CreateTool(Hammer));
            GameObject.AddTemplate(ItemFactory.CreateTool(Axe));
            GameObject.AddTemplate(ItemFactory.CreateTool(Pickaxe));
            GameObject.AddTemplate(ItemFactory.CreateTool(Handsaw));
            GameObject.AddTemplate(ItemFactory.CreateTool(Hoe));

            Reaction.Register(Reaction.Repairing);

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
                    SkillDef.Crafting);
            }
        }
    }
}
 
