using Start_a_Town_.Components;
using Start_a_Town_.UI;
using System.IO;

namespace Start_a_Town_
{
    class UnfinishedItemComp : EntityComponent
    {
        public override string Name => "UnfinishedItem";

        public Reaction.Product.ProductMaterialPair Product;
        public Progress Progress = new();
        int _creator, _orderid;
        Actor _creatorCached;
        public Actor Creator => this._creatorCached ??= this.Parent.Net.GetNetworkObject<Actor>(this._creator);
        CraftOrder _orderCached;
        public CraftOrder Order => this._orderCached ??= this.Parent.Net.Map.Town.CraftingManager.GetOrder(this._orderid);
        public override object Clone()
        {
            return new UnfinishedItemComp();
        }

        internal void SetProduct(Reaction.Product.ProductMaterialPair product, Actor creator, CraftOrder order)
        {
            this._orderCached = order;
            this._creatorCached = creator;
            this.Product = product;
            this.Progress.Max = product.WorkAmount;
            this.Parent.Physics.SetWeight(product.Product.Physics.Weight);
            this.Parent.Name = $"Unfinished {product.Product.Def.Label}";
            ((Entity)this.Parent).SetMaterial(product.Product.PrimaryMaterial);
        }

        internal override void GetSelectionInfo(IUISelection info, GameObject parent)
        {
            var box = new GroupBox();
            box.AddControlsVertically(
                this.Progress.GetGui(),
                new Label($"Creator: {this.Creator.Name}"),
                UI.Label.ParseNewNew("Order: ", this.Order).ToGroupBoxHorizontally()
                );
            info.AddInfo(box);
        }

        internal override void SaveExtra(SaveTag tag)
        {
            this.Product.Save(tag, "Product");
            this.Creator.RefID.Save(tag, "Creator");
            this.Order.ID.Save(tag, "Order");
            this.Progress.Save(tag, "Progress");
        }
        internal override void LoadExtra(SaveTag tag)
        {
            this.Product = new(tag["Product"]);
            this._creator = (int)tag["Creator"].Value;
            this._orderid = (int)tag["Order"].Value;
            this.Progress.Load(tag["Progress"]);
        }
        public override void Write(BinaryWriter w)
        {
            this.Product.Write(w);
            this.Progress.Write(w);
            w.Write(this.Creator.RefID);
            w.Write(this.Order.ID);
        }

        public override void Read(BinaryReader r)
        {
            this.Product = new(r);
            this.Progress.Read(r);
            this._creator = r.ReadInt32();
            this._orderid = r.ReadInt32();
        }
        public override void OnDispose()
        {
            this.Order.UnfinishedItem = null;
        }
    }
}
