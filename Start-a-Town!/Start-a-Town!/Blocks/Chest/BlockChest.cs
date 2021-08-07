using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Start_a_Town_.Blocks.Chest
{
    partial class BlockChest : Block
    {
        static Texture2D ChestNormalMap = Game1.Instance.Content.Load<Texture2D>("graphics/items/blocks/furniture/chestnormal");
        public BlockChest()
            : base("Chest", opaque: false)
        {
            var tex = Game1.Instance.Content.Load<Texture2D>("graphics/items/blocks/furniture/chest").ToGrayscale();
            this.Variations.Add(Atlas.Load("chestgrayscale", tex, BlockDepthMap, ChestNormalMap));
            this.ToggleConstructionCategory(ConstructionsManager.Furniture, true);
        }
        public override IEnumerable<MaterialDef> GetEditorVariations()
        {
            return Def.GetDefs<MaterialDef>().Where(mat => mat.Type == MaterialType.Wood || mat.Type == MaterialType.Metal);
        }
        public override BlockEntity CreateBlockEntity(IntVec3 originGlobal)
        {
            return new BlockChestEntity(originGlobal, 16);
        }
    }
}
