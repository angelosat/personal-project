using System;
using System.Collections.Generic;
using System.Linq;
using Start_a_Town_.Blocks;
using Start_a_Town_.Graphics;

namespace Start_a_Town_
{
    sealed class BlockWorkstation : BlockWithEntity, IBlockWorkstation
    {
        readonly AtlasDepthNormals.Node.Token[] Orientations = Block.TexturesCounter;
        readonly Type BlockEntityType;
        public override bool IsDeconstructible => true;

        public BlockWorkstation(string name, Type blockEntityType)
            : base(name, opaque: false, solid: true)
        {
            this.BlockEntityType = blockEntityType;
            this.Variations.Add(this.Orientations.First());
            this.ToggleConstructionCategory(ConstructionsManager.Production, true);
            this.Ingredient = new Ingredient(amount: 4).IsBuildingMaterial();
            this.Ingredient.MaterialVolume = 1 / 4f;
        }
        public override AtlasDepthNormals.Node.Token GetToken(int variation, int orientation, int cameraRotation, byte data)
        {
            return this.Orientations[orientation];
        }
        public override BlockEntity CreateBlockEntity(IntVec3 originGlobal)
        {
            return Activator.CreateInstance(this.BlockEntityType, originGlobal) as BlockEntity;
        }
        internal override IEnumerable<IntVec3> GetOperatingPositions(int orientation)
        {
            yield return Cell.GetFront(orientation);
        }
    }
}
