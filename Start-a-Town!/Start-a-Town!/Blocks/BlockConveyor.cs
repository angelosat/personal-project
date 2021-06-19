using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.Graphics;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.Blocks
{
    class BlockConveyor : Block
    {
        public BlockConveyor()
            : base(Block.Types.Conveyor, opaque: false)
        {
            var txt = Block.Atlas.Load("blocks/slab", Block.QuarterBlockMapDepth, Block.QuarterBlockMapNormal);
            this.Variations.Add(txt);
        }
        public override Vector3 GetVelocityTransform(byte data, Vector3 blockcoords)
        {
            return Vector3.UnitX * .05f;
        }
        public override float GetPathingCost(byte data)
        {
            return 0;
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
            return this.GetHeight(x, y);
        }
        public override float GetHeight(float x, float y)
        {
            return .25f;
        }
        public override IEnumerable<byte> GetCraftingVariations()
        {
            yield return (byte)MaterialDefOf.Stone.ID;
            //return new List<byte>() { (byte)MaterialDefOf.Stone.ID };
        }
        public override Material GetMaterial(byte blockdata)
        {
            return MaterialDefOf.Stone;
        }
    }
}
