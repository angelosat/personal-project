using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.Blocks;
using Start_a_Town_.UI;
using Start_a_Town_.Net;
using System.IO;
using Start_a_Town_.Graphics;
using Start_a_Town_.Towns;

namespace Start_a_Town_
{
    class BlockShopCounter : Block, IBlockWorkstation
    {
        AtlasDepthNormals.Node.Token[] Orientations = TexturesCounter;

        //static BlockShopCounter()
        //{
        //    Init();
        //}
        public BlockShopCounter() : base(Block.Types.ShopCounter, 0, 1, false, true)
        {
            this.Variations.Add(this.Orientations.First());
            this.Furniture = FurnitureDefOf.Counter;
        }

        public override Material GetMaterial(byte blockdata)
        {
            return Material.GetMaterial(blockdata);
        }
        internal override void GetSelectionInfo(IUISelection info, IMap map, Vector3 vector3)
        {
            var shop = map.Town.ShopManager.GetShops().FirstOrDefault(s => s.Counter.HasValue && s.Counter.Value == vector3);
            info.AddInfo(new Label($"Shop: {shop?.Name ?? "none"}"));
        }
        //internal override void GetQuickButtons(UISelectedInfo uISelectedInfo, IMap map, Vector3 vector3)
        //{
        //    return;
        //    uISelectedInfo.AddTabAction("Shop", () =>
        //        map.Town.ShopManager.GetUIShopListWithNoneOption(s => map.Town.ShopManager.PlayerAssignCounter(s as Shop, vector3)).SetLocation(UIManager.Mouse).Toggle());
        //}
    }
}

