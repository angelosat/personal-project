using Start_a_Town_.Components;
using System;

namespace Start_a_Town_
{
    class UnfinishedItemComp : EntityComponent
    {
        public override string Name => "UnfinishedItem";

        public Reaction.Product.ProductMaterialPair Product;
        public Progress Progress = new();
        public Actor Creator;
        public CraftOrder Order;
        public override object Clone()
        {
            return new UnfinishedItemComp();
        }

        internal void SetProduct(Reaction.Product.ProductMaterialPair product, int workRequired, Actor creator, CraftOrder order)
        {
            this.Order = order;
            this.Creator = creator;
            this.Product = product;
            this.Progress.Max = workRequired;
            this.Parent.Physics.SetWeight(product.Product.Physics.Weight);
            this.Parent.Name = $"Unfinished {product.Product.Def.Label}";
            //this.Parent.SpriteComp.Body = product.Product.Body;
            ((Entity)this.Parent).SetMaterial(product.Product.PrimaryMaterial);
        }
    }
}
