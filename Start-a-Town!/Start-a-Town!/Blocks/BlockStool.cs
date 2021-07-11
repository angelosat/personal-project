using Start_a_Town_.Components.Crafting;

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
            this.Variations.Add(Block.Atlas.Load("blocks/furniture/stool", MapBase.BlockDepthMap, Block.NormalMap));
            this.Furniture = FurnitureDefOf.Table;
            
            this.Recipe = new BlockRecipe(
                Reaction.Reagent.Create(
                    new Reaction.Reagent()),
                    new BlockRecipe.Product(this),
                    ToolAbilityDef.Building);
            Towns.Constructions.ConstructionsManager.Furniture.Add(this.Recipe);
            this.Ingredient = new Ingredient(amount: 4).IsBuildingMaterial();

        }
        
        public override void Place(MapBase map, IntVec3 global, byte data, int variation, int orientation, bool notify = true)
        {
            base.Place(map, global, data, variation, orientation, notify);
            map.Town.AddUtility(Utility.Types.Eating, global);
        }
        public override void Remove(MapBase map, IntVec3 global, bool notify = true)
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