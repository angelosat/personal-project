using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Start_a_Town_.Blocks.Chest
{
    partial class BlockChest : Block
    {
        static Texture2D ChestNormalMap = Game1.Instance.Content.Load<Texture2D>("graphics/items/blocks/furniture/chestnormal");
        public override Material GetMaterial(byte blockdata)
        {
            return Material.Registry[blockdata];
        }
        public BlockChest()
            : base(Block.Types.Chest, opaque: false)
        {
            var tex = Game1.Instance.Content.Load<Texture2D>("graphics/items/blocks/furniture/chest").ToGrayscale();
            this.Variations.Add(Block.Atlas.Load("chestgrayscale", tex, MapBase.BlockDepthMap, ChestNormalMap));
            this.ToggleConstructionCategory(ConstructionsManager.Furniture, true);
        }

        public override IEnumerable<byte> GetCraftingVariations()
        {
            var vars = (from mat in Material.Registry.Values
                        where mat.Type == MaterialType.Wood || mat.Type == MaterialType.Metal
                        select (byte)mat.ID);
            return vars;
        }
        public override BlockEntity CreateBlockEntity()
        {
            return new BlockChestEntity(16);
        }
        public override Vector4 GetColorVector(byte data)
        {
            var mat = Material.Registry[data];
            var c = mat.ColorVector;
            return c;
        }
    }
}
