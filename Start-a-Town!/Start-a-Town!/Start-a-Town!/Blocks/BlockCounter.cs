﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.Graphics;
using Start_a_Town_.Components.Materials;

namespace Start_a_Town_.Blocks
{
    class BlockCounter : Block
    {
        public override Material GetMaterial(byte blockdata)
        {
            return Material.LightWood;
        }
        AtlasDepthNormals.Node.Token[] Orientations = new AtlasDepthNormals.Node.Token[4];
        public BlockCounter():base(Block.Types.Counter, opaque: false)
        {
            //this.Material = Material.LightWood;
            //this.MaterialType = MaterialType.Wood;
            this.Orientations[0] = Block.Atlas.Load("blocks/counters/counter1");
            this.Orientations[1] = Block.Atlas.Load("blocks/counters/counter4");
            this.Orientations[2] = Block.Atlas.Load("blocks/counters/counter3");
            this.Orientations[3] = Block.Atlas.Load("blocks/counters/counter2");
            this.Variations.Add(this.Orientations.First());
        }
        public override List<byte> GetVariations()
        {
            return new List<byte>() { 0 };
        }
        public override AtlasDepthNormals.Node.Token GetToken(int variation, int orientation, int cameraRotation, byte data)
        {
            return this.Orientations[(orientation + (int)cameraRotation) % 4];
        }
    }
}
