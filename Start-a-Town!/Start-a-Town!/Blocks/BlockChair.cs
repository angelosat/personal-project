using Start_a_Town_.Components.Crafting;
using Start_a_Town_.Graphics;

namespace Start_a_Town_.Blocks
{
    class BlockChair : Block
    {
        static AtlasDepthNormals.Node.Token[] Orientations = new AtlasDepthNormals.Node.Token[4];

        public override Material GetMaterial(byte blockdata)
        {
            return MaterialDefOf.LightWood;
        }
        
        public BlockChair():base(Block.Types.Chair, opaque: false)
        {
            Orientations[0] = Block.Atlas.Load("blocks/furniture/chair", Block.HalfBlockDepthMap, Block.NormalMap);
            Orientations[1] = Block.Atlas.Load("blocks/furniture/chair2", Block.HalfBlockDepthMap, Block.NormalMap);
            Orientations[2] = Block.Atlas.Load("blocks/furniture/chairback2", Block.HalfBlockDepthMap, Block.NormalMap);
            Orientations[3] = Block.Atlas.Load("blocks/furniture/chairback", Block.HalfBlockDepthMap, Block.NormalMap);

            this.Recipe = new BlockRecipe(
                Reaction.Reagent.Create(
                    new Reaction.Reagent(
                        "Base",
                        Reaction.Reagent.CanProduce(Reaction.Product.Types.Blocks))),
                    new BlockRecipe.Product(this),
                    ToolAbilityDef.Building);
            Towns.Constructions.ConstructionsManager.Furniture.Add(this.Recipe);
            this.Ingredient = new Ingredient(amount: 4).IsBuildingMaterial();
        }
        public override void Place(MapBase map, IntVec3 global, byte data, int variation, int orientation, bool notify = true)
        {
            base.Place(map, global, data, orientation, 0, notify);
        }
        public override float GetHeight(float x, float y)
        {
            return 0.5f;
        }
        public override AtlasDepthNormals.Node.Token GetToken(int variation, int orientation, int cameraRotation, byte data)
        {
            return Orientations[orientation];
        }
        
        public override AtlasDepthNormals.Node.Token GetDefault()
        {
            return Orientations[0];
        }
        public override MyVertex[] Draw(Canvas canvas, Chunk chunk, Microsoft.Xna.Framework.Vector3 blockCoordinates, Camera camera, Microsoft.Xna.Framework.Vector4 screenBounds, Microsoft.Xna.Framework.Color sunlight, Microsoft.Xna.Framework.Vector4 blocklight, Microsoft.Xna.Framework.Color fog, Microsoft.Xna.Framework.Color tint, float depth, int variation, int orientation, byte data)
        {
            DrawShadow(canvas.NonOpaque, blockCoordinates, camera, screenBounds, sunlight, blocklight, fog, tint, depth);
            return base.Draw(canvas, chunk, blockCoordinates, camera, screenBounds, sunlight, blocklight, fog, tint, depth, 0, variation, data);
        }
    }
}
