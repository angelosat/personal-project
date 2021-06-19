using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.Components.Crafting;
using Start_a_Town_.Blocks;

namespace Start_a_Town_
{
    class BlockStool : Block
    {
        public override Material GetMaterial(byte blockdata)
        {
            return MaterialDefOf.LightWood;
        }

        public BlockStool()
            : base(Block.Types.Stool, opaque: false)
        {
            //this.MaterialType = MaterialType.Wood;
            //this.AssetNames = "furniture/stool";
            this.Variations.Add(Block.Atlas.Load("blocks/furniture/stool", Map.BlockDepthMap, Block.NormalMap));
            //this.Material = Material.LightWood;
            this.Furniture = FurnitureDefOf.Table;
            //this.Recipe = new BlockRecipe(
            //    Reaction.Reagent.Create(
            //        new Reaction.Reagent(
            //            "Base",
            //    //Reaction.Reagent.IsOfMaterialType(MaterialType.Wood), 
            //            Reaction.Reagent.IsOfSubType(ItemSubType.Planks),
            //            Reaction.Reagent.CanProduce(Reaction.Product.Types.Blocks)) { Quantity = 2 }),
            //        new BlockRecipe.Product(this),
            //        Components.Skills.Skill.Building);

            this.Recipe = new BlockRecipe(
                Reaction.Reagent.Create(
                    new Reaction.Reagent(
                        "Base",
                        Reaction.Reagent.IsOfSubType(ItemSubType.Planks, ItemSubType.Ingots)
                        )),
                    new BlockRecipe.Product(this),
                    ToolAbilityDef.Building);
            Towns.Constructions.ConstructionsManager.Furniture.Add(this.Recipe);
            this.Ingredient = new Ingredient(amount: 4).IsBuildingMaterial();

        }
        //public override float GetHeight(float x, float y)
        //{
        //    return 0.5f;
        //}
        public override void Place(IMap map, Microsoft.Xna.Framework.Vector3 global, byte data, int variation, int orientation, bool notify = true)
        {
            base.Place(map, global, data, variation, orientation, notify);
            map.Town.AddUtility(Utility.Types.Eating, global);
        }
        public override void Remove(IMap map, Microsoft.Xna.Framework.Vector3 global, bool notify = true)
        {
            base.Remove(map, global, notify);
            map.Town.RemoveUtility(Utility.Types.Eating, global);
        }
        public override MyVertex[] Draw(Canvas canvas, Chunk chunk, Microsoft.Xna.Framework.Vector3 blockCoordinates, Camera camera, Microsoft.Xna.Framework.Vector4 screenBounds, Microsoft.Xna.Framework.Color sunlight, Microsoft.Xna.Framework.Vector4 blocklight, Microsoft.Xna.Framework.Color fog, Microsoft.Xna.Framework.Color tint, float depth, int variation, int orientation, byte data)
        {
            DrawShadow(canvas.NonOpaque, blockCoordinates, camera, screenBounds, sunlight, blocklight, fog, tint, depth);
            return base.Draw(canvas, chunk, blockCoordinates, camera, screenBounds, sunlight, blocklight, fog, tint, depth, variation, orientation, data);
        }
    }
}