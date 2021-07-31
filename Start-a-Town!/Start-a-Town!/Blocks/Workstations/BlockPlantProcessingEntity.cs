using Start_a_Town_.Blocks;

namespace Start_a_Town_
{
    class BlockPlantProcessingEntity : BlockEntity
    {
        public BlockPlantProcessingEntity(IntVec3 originGlobal)
            : base(originGlobal)
        {
            this.Comps.Add(new BlockEntityCompWorkstation(IsWorkstation.Types.PlantProcessing));
            this.Comps.Add(new BlockEntityCompDeconstructible());
        }
    }
}
