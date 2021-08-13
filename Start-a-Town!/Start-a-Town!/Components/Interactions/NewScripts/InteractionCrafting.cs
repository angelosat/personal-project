using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Start_a_Town_.Net;

namespace Start_a_Town_.Crafting
{
    class InteractionCrafting : InteractionToolUse
    {
        CraftOrder Order;
        Reaction.Product.ProductMaterialPair Product;
        readonly List<ObjectRefIDsAmount> PlacedObjects = new();
        Progress _progress = new();

        protected override float Progress => this._progress.Percentage;
        protected override float WorkDifficulty => 1;
        protected override SkillAwardTypes SkillAwardType { get; } = SkillAwardTypes.OnFinish;

        int _orderID;
        private int OrderID => this.Order?.ID ?? this._orderID;

        public InteractionCrafting(CraftOrder order, List<ObjectAmount> placedObjects)
            : this()
        {
            this.Order = order;
            this.PlacedObjects = placedObjects.Select(o => new ObjectRefIDsAmount(o.Object, o.Amount)).ToList();
        }
        public InteractionCrafting(CraftOrder order, List<ObjectRefIDsAmount> placedObjects)
            : this()
        {
            this.Order = order;
            this.PlacedObjects = placedObjects;
        }
        public InteractionCrafting()
            : base("Produce")
        {
            this.DrawProgressBar(() => this.Target.Global.Above(), () => this.Progress, () => this.Order.Label);
        }

        public override object Clone()
        {
            return new InteractionCrafting(this.Order, this.PlacedObjects);
        }
        protected override void Init()
        {
            var actor = this.Actor;
            var ingr = this.PlacedObjects.Select(o => new ObjectAmount(actor.Net.GetNetworkObject(o.Object), o.Amount)).ToList();
            this.Product = this.Order.Reaction.Products.First().Make(actor, this.Order.Reaction, ingr);
            this._progress.Max = this.Product.WorkAmount;
            this._progress.Max.ToConsole();
        }
        GameObject ProduceWithMaterialsOnTopNew()
        {
            var global = this.Target.Global;
            var actor = this.Actor;
            var product = this.Product;
            var reaction = this.Order.Reaction;
            var order = this.Order;
            //var ingr = this.PlacedObjects.Select(o => new ObjectAmount(actor.Net.GetNetworkObject(o.Object), o.Amount)).ToList();
            //var reaction = order.Reaction;
            //var product = reaction.Products.First().Make(actor, reaction, ingr);
            var skillAwardAmount = 100;
            actor.AwardSkillXP(reaction.CraftSkill, skillAwardAmount);

            product.ConsumeMaterials();
            if (order.Reaction.Fuel > 0)
                actor.Map.GetBlockEntity(global)?.GetComp<BlockEntityCompRefuelable>()?.ConsumePower(actor.Map, order.Reaction.Fuel);

            order.Complete(actor);

            if (actor.Net is not Server server)
                return null;
            if (!product.Product.Exists) // HACK for when the product is one of the ingredients (already existing on top of the workstation)
            {
                product.Product.Global = global + Vector3.UnitZ;
                product.Product.SyncInstantiate(server);
                actor.Map.SyncSpawn(product.Product, global.Above(), Vector3.Zero);
            }
            return product.Product;
        }

        protected override void AddSaveData(SaveTag tag)
        {
            tag.TrySaveRef(this.Order, "Order");
            tag.Add(this.Progress.Save("CraftProgress"));
            tag.Add(this.PlacedObjects.SaveNewBEST("PlacedItems"));
            this.Product.Save(tag, "Product");
        }
        public override void LoadData(SaveTag tag)
        {
            tag.TryLoadRef("Order", out this.Order);
            this._progress = new Progress(tag["CraftProgress"]);
            this.PlacedObjects.TryLoadMutable(tag, "PlacedItems");
            this.Product = new Reaction.Product.ProductMaterialPair(tag["Product"]);
        }
        protected override void WriteExtra(BinaryWriter w)
        {
            w.Write(this.Order.ID);
            this._progress.Write(w);
            this.PlacedObjects.Write(w);
            this.Product.Write(w);
        }
        protected override void ReadExtra(BinaryReader r)
        {
            var orderID = r.ReadInt32();
            if (this.Actor.Map is not null)
                this.Order = this.Actor.Map.GetBlockEntity(this.Target.Global).GetComp<BlockEntityCompWorkstation>().GetOrder(orderID);
            else
                this._orderID = orderID;
            this._progress = new Progress(r);
            this.PlacedObjects.ReadMutable(r);
            this.Product = new Reaction.Product.ProductMaterialPair(r);
        }

        internal override void ResolveReferences()
        {
            this.Order = this.Actor.Map.GetBlockEntity(this.Target.Global).GetComp<BlockEntityCompWorkstation>().GetOrder(this.OrderID);
        }

        protected override void ApplyWork(float workAmount)
        {
            this._progress.Value += workAmount;// 25;
        }

        protected override void Done()
        {
            var a = this.Actor;
            var t = this.Target;
            var product = this.ProduceWithMaterialsOnTopNew();
            if (a.Net is Server)
            {
                var task = a.CurrentTask;
                if (task is not null)
                    task.Product = new TargetArgs(product);
            }
        }

        protected override ToolUseDef GetToolUse()
        {
            return this.Order.Reaction.Labor?.ToolUse;
        }

        protected override SkillDef GetSkill()
        {
            return this.Order.Reaction.CraftSkill;
        }

        protected override List<Rectangle> GetParticleRects()
        {
            return null;
        }

        protected override Color GetParticleColor()
        {
            return default;
        }
    }
}
