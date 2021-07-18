﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Start_a_Town_.Blocks;
using Start_a_Town_.Graphics;

namespace Start_a_Town_
{
    sealed class BlockWorkstation : BlockWithEntity, IBlockWorkstation
    {
        AtlasDepthNormals.Node.Token[] Orientations = Block.TexturesCounter;
        Type BlockEntityType;
        public BlockWorkstation(Block.Types workstationType, Type blockEntityType)
            : base(workstationType, opaque: false, solid: true)
        {
            this.BlockEntityType = blockEntityType;
            this.Variations.Add(this.Orientations.First());
            this.ToggleConstructionCategory(ConstructionsManager.Production, true);
            this.Ingredient = new Ingredient(amount: 4).IsBuildingMaterial();
        }
        public override AtlasDepthNormals.Node.Token GetToken(int variation, int orientation, int cameraRotation, byte data)
        {
            return this.Orientations[orientation];
        }
        public override BlockEntity CreateBlockEntity()
        {
            return Activator.CreateInstance(this.BlockEntityType) as BlockEntity;
        }

        public override Material GetMaterial(byte blockdata)
        {
            return Material.Registry[blockdata];
        }
       
        public override Vector4 GetColorVector(byte data)
        {
            return this.GetColorFromMaterial(data);
        }
    }
}
