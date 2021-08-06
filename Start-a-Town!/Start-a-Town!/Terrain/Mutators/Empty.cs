using Start_a_Town_.GameModes;

namespace Start_a_Town_.Terraforming.Mutators
{
    class Empty : Terraformer
    {
        public Empty()
        {
            this.ID = Terraformer.Types.Empty;
            this.Name = "Empty";
        }
        public override Block Initialize(IWorld w, Cell c, int x, int y, int z, Net.RandomThreaded r)
        {
            return BlockDefOf.Air;
        }
        public override object Clone()
        {
            return new Empty();
        }
    }
}
