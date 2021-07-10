using Start_a_Town_.Blocks;

namespace Start_a_Town_
{
    public class BlockWorkbenchEntity : BlockEntity
    {
        public BlockWorkbenchEntity()
        {
            this.Comps.Add(new BlockEntityCompWorkstation(IsWorkstation.Types.Workbench));
            this.Comps.Add(new BlockEntityCompDeconstructible());
        }

        public override object Clone()
        {
            return new BlockWorkbenchEntity();
        }
    }
}
