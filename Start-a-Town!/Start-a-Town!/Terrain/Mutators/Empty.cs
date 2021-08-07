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
        public override object Clone()
        {
            return new Empty();
        }
    }
}
