﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components.Crafting;
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
            this.Recipe = new BlockRecipe(
                Reaction.Reagent.Create(
                    new Reaction.Reagent(
                        "Base",
                        Reaction.Reagent.IsOfMaterialType(MaterialType.Wood),
                        Reaction.Reagent.CanProduce(Reaction.Product.Types.Blocks))),
                    new BlockRecipe.Product(this),
                    ToolAbilityDef.Building)
            { WorkAmount = 20 };

            Towns.Constructions.ConstructionsManager.Production.Add(this.Recipe);
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

        public override BlockRecipe GetRecipe()
        {
            return this.Recipe;
        }
        public override Material GetMaterial(byte blockdata)
        {
            return Material.Database[blockdata];
        }
        public override IEnumerable<byte> GetCraftingVariations()
        {
            return this.Ingredient.AllowedMaterials.Select(m => (byte)m.ID);
        }
        public override Vector4 GetColorVector(byte data)
        {
            return this.GetColorFromMaterial(data);
        }
    }
}
