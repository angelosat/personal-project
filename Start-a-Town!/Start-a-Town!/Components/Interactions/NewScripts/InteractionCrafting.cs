using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.Net;
using Start_a_Town_.UI;

namespace Start_a_Town_.Crafting
{
    class InteractionCrafting : InteractionPerpetual
    {
        CraftOrder Order;
        Progress Progress = new();
        int _orderID;
        private int OrderID => this.Order?.ID ?? this._orderID;
        readonly List<ObjectRefIDsAmount> PlacedObjects = new();
        readonly int BaseWorkAmount = 5;//25;

        public InteractionCrafting(CraftOrder order, List<ObjectAmount> placedObjects)
            : this()
        {
            this.Order = order;
            this.PlacedObjects = placedObjects.Select(o=>new ObjectRefIDsAmount(o.Object, o.Amount)).ToList();
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
            this.DrawProgressBar(() => this.Target.Global.Above(), () => this.Progress.Percentage, () => this.Order.Name);
        }

        public override void Perform()
        {
            var a = this.Actor;
            var t = this.Target;
            if (this.Progress.Value >= this.Progress.Max)
            {
                var product = ProduceWithMaterialsOnTopNew(a, t.Global, this.Order);

                if (a.Net is Server)
                {
                    var task = a.CurrentTask;
                    if (task != null)
                        task.Product = new TargetArgs(product);
                }
                this.Finish();
            }
        }
        internal override void OnToolContact()
        {
            var a = this.Actor;
            var t = this.Target;
            var work = StatDefOf.ToolEffectiveness.GetValue(a);
            this.Progress.Value += work * BaseWorkAmount;// 25;
        }
        
        public override object Clone()
        {
            return new InteractionCrafting(this.Order, this.PlacedObjects);

        }

        GameObject ProduceWithMaterialsOnTopNew(Actor a, Vector3 global, CraftOrder order)
        {
            var actor = a as Actor;
            var ingr = this.PlacedObjects.Select(o => new ObjectAmount(actor.Net.GetNetworkObject(o.Object), o.Amount)).ToList();
            
            var reaction = order.Reaction;
            var product = reaction.Products.First().Make(actor, reaction, ingr);
            var skillAwardAmount = 100;
            actor.AwardSkillXP(reaction.CraftSkill, skillAwardAmount);

            product.ConsumeMaterials();
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
        }
        public override void LoadData(SaveTag tag)
        {
            tag.TryLoadRef("Order", out this.Order);
            this.Progress = new Progress(tag["CraftProgress"]);
            this.PlacedObjects.TryLoadMutable(tag, "PlacedItems");
        }
        protected override void WriteExtra(BinaryWriter w)
        {
            w.Write(this.Order.ID);
            this.Progress.Write(w);
            this.PlacedObjects.Write(w);
        }
        protected override void ReadExtra(BinaryReader r)
        {
            var orderID = r.ReadInt32();
            if (this.Actor.Map is not null)
                this.Order = this.Actor.Map.GetBlockEntity(this.Target.Global).GetComp<BlockEntityCompWorkstation>().GetOrder(orderID);
            else
                this._orderID = orderID;
            this.Progress = new Progress(r);
            this.PlacedObjects.ReadMutable(r);
        }

        internal override void ResolveReferences()
        {
            this.Order = this.Actor.Map.GetBlockEntity(this.Target.Global).GetComp<BlockEntityCompWorkstation>().GetOrder(this.OrderID);
        }

        public override void OnUpdate()
        {
            throw new NotImplementedException();
        }
    }
}
