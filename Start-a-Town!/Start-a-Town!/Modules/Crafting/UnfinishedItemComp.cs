using Start_a_Town_.Components;

namespace Start_a_Town_
{
    class UnfinishedItemComp : EntityComponent
    {
        public override string Name => "UnfinishedItem";

        public Reaction.Product.ProductMaterialPair Product;

        public override object Clone()
        {
            return new UnfinishedItemComp();
        }
    }
}
