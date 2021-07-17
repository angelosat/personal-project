using System.Linq;
using Microsoft.Xna.Framework;
using Start_a_Town_.UI;
using Start_a_Town_.Graphics;

namespace Start_a_Town_
{
    class BlockShopCounter : Block, IBlockWorkstation
    {
        AtlasDepthNormals.Node.Token[] Orientations = TexturesCounter;

        public BlockShopCounter() : base(Block.Types.ShopCounter, 0, 1, false, true)
        {
            this.Variations.Add(this.Orientations.First());
            this.Furniture = FurnitureDefOf.Counter;
        }

        public override Material GetMaterial(byte blockdata)
        {
            return Material.GetMaterial(blockdata);
        }
        internal override void GetSelectionInfo(IUISelection info, MapBase map, IntVec3 vector3)
        {
            var shop = map.Town.ShopManager.GetShops().FirstOrDefault(s => s.Counter.HasValue && s.Counter.Value == vector3);
            info.AddInfo(new Label($"Shop: {shop?.Name ?? "none"}"));
        }
    }
}

