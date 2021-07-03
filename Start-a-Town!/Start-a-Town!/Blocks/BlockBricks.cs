using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components.Crafting;

namespace Start_a_Town_
{
    class BlockBricks : Block
    {
        public BlockBricks()
            : base(Types.Bricks)
        {
            this.AssetNames = "bricks/bricks";
            this.Recipe = new BlockRecipe(
                Reaction.Reagent.Create(
                    new Reaction.Reagent(
                        "Base",
                        Reaction.Reagent.IsOfSubType(ItemSubType.Planks, ItemSubType.Ingots, ItemSubType.Rock)
                        )),
                    new BlockRecipe.Product(this),
                    ToolAbilityDef.Building) { WorkAmount = 20 };
            Towns.Constructions.ConstructionsManager.Walls.Add(this.Recipe);
            this.Ingredient = new Ingredient(RawMaterialDef.Boulders, null, null, 1);
        }

        public override IEnumerable<byte> GetCraftingVariations()
        {
            return (from mat in Material.Database.Values
                    where mat.Type == MaterialType.Stone
                    select (byte)mat.ID);

        }
        public override Material GetMaterial(byte data)
        {
            return Material.Database[data];
        }
        public override byte GetDataFromMaterial(GameObject craftingReagent)
        {
            return (byte)craftingReagent.Body.Material.ID;
        }
        public override Color GetColor(byte data)
        {
            var mat = Material.Database[data];
            var c = mat.Color;
            c.A = (byte)(255*mat.Type.Shininess);
            return c;
        }
        public override Vector4 GetColorVector(byte data)
        {
            var mat = Material.Database[data];
            var c = mat.ColorVector;
            return c;
        }
        public override byte ParseData(string data)
        {
            var mat = Material.Database.Values.FirstOrDefault(m => string.Equals(m.Name, data, StringComparison.OrdinalIgnoreCase));
            if (mat == null)
                return (byte)MaterialDefOf.Stone.ID;
            return (byte)mat.ID;
        }
    }
}
