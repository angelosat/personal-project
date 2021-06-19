using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.Net;
using Start_a_Town_.UI;
using Start_a_Town_.Graphics;

namespace Start_a_Town_.Blocks
{
    partial class BlockPrefab : Block
    {
        static readonly AtlasDepthNormals.Node.Token Token = Block.Atlas.Load("blocks/blockblueprint");
        public BlockPrefab()
            : base(Block.Types.Prefab, 0, 1, true, true)
        {

        }
        public override AtlasDepthNormals.Node.Token GetToken(int variation, int orientation, int cameraRotation, byte data)
        {
            return Token;
        }
        public override Material GetMaterial(byte blockdata)
        {
            return MaterialDefOf.LightWood;
        }
    }
}
