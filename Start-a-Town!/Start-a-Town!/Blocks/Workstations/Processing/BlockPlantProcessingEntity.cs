using Start_a_Town_.Blocks;
using Start_a_Town_.Tokens;

namespace Start_a_Town_
{
    
        class BlockPlantProcessingEntity : BlockEntity//Workstation
        {
            public BlockPlantProcessingEntity()
            {
                this.Comps.Add(new BlockEntityCompWorkstation(IsWorkstation.Types.PlantProcessing));
            this.Comps.Add(new BlockEntityCompDeconstructible());
            }

            public override object Clone()
            {
                return new BlockPlantProcessingEntity();
            }
            //public override IsWorkstation.Types Type { get { return IsWorkstation.Types.PlantProcessing; } }
            //public Container Storage;
            //public Entity()
            //{
            //    this.Storage = new Container(8);
            //}
            //public override Container Input
            //{
            //    get
            //    {
            //        return this.Storage;
            //    }
            //}
            //public override object Clone()
            //{
            //    return new Entity();
            //}
        }
}
