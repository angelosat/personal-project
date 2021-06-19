using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.Graphics;
using Start_a_Town_.Components.Materials;

namespace Start_a_Town_.Blocks
{
    class BlockSlab : Block
    {
        //AtlasDepthNormals.Node.Token Sprite = Block.LoadTexture("counter1grayscale", "/counters/counter1");

        public BlockSlab()
            : base(Block.Types.Slab, opaque: false)
        {
            var txt = Block.Atlas.Load("blocks/slab", Block.QuarterBlockMapDepth, Block.QuarterBlockMapNormal);
            this.Variations.Add(txt);  
        }
        public override float GetPathingCost(byte data)
        {
            return 0;// .1f;
        }
        public override Microsoft.Xna.Framework.Color[] UV
        {
            get
            {
                return Block.BlockCoordinatesQuarter;
            }
        }
        public override MouseMap MouseMap
        {
            get
            {
                return Block.BlockQuarterMouseMap;
            }
        }
        public override float GetHeight(byte data, float x, float y)
        {
            return .25f;
        }
        public override List<byte> GetVariations()
        {
            return new List<byte>() { (byte)Material.Stone.ID };
        }
        public override Material GetMaterial(byte blockdata)
        {
            return Material.Stone;
        }
    }
}
