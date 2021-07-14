using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Start_a_Town_.Net;

namespace Start_a_Town_.Crafting
{
    class InteractionCraftNew : InteractionPerpetual
    {
        int OrderID;
        Progress Progress = new();
        readonly List<ObjectRefIDsAmount> PlacedObjects = new();

        public InteractionCraftNew(int orderID, List<ObjectAmount> placedObjects)
            : base("Produce")
        {
            this.OrderID = orderID;
            this.PlacedObjects = placedObjects.Select(o=>new ObjectRefIDsAmount(o.Object, o.Amount)).ToList();
        }
        public InteractionCraftNew(int orderID, List<ObjectRefIDsAmount> placedObjects)
            : base("Produce")
        {
            this.OrderID = orderID;
            this.PlacedObjects = placedObjects;
        }
        public InteractionCraftNew()
            : base("Produce")
        {

        }

        readonly int BaseWorkAmount = 5;//25;
        
        public override void Start(Actor a, TargetArgs t)
        {
            base.Start(a, t);
        }
        public override void Perform(Actor a, TargetArgs t)
        {
            if (this.Progress.Value >= this.Progress.Max)
            {
                var orderContainer = a.Map.GetBlockEntity(t.Global).GetComp<BlockEntityCompWorkstation>();
                var order = orderContainer.GetOrder(this.OrderID);

                var product = ProduceWithMaterialsOnTopNew(a, t.Global, order);


                if (a.Net is Server)
                {
                    var task = a.CurrentTask;
                    if (task != null)
                        task.Product = new TargetArgs(product);
                }
                this.Finish(a, t);
            }
        }
        internal override void OnToolContact(Actor a, TargetArgs t)
        {
            var work = StatDefOf.ToolEfficiency.GetValue(a);
            this.Progress.Value += work * BaseWorkAmount;// 25;
        }
        
        
        public override object Clone()
        {
            return new InteractionCraftNew(this.OrderID, this.PlacedObjects);

        }

        GameObject ProduceWithMaterialsOnTopNew(Actor a, Vector3 global, CraftOrderNew order)
        {
            var actor = a as Actor;
            var ingr = this.PlacedObjects.Select(o => new ObjectAmount(actor.Net.GetNetworkObject(o.Object), o.Amount)).ToList();
            
            var reaction = order.Reaction;
            var product = reaction.Products.First().Make(actor, reaction, ingr);
            var skillAwardAmount = 100;
            actor.AwardSkillXP(reaction.CraftSkill, skillAwardAmount);

            product.ConsumeMaterials();
            actor.Map.GetBlockEntity(global)?.GetComp<BlockEntityCompRefuelable>()?.ConsumePower(actor.Map, order.Reaction.Fuel);

            actor.Net.Map.EventOccured(Components.Message.Types.CraftingComplete, actor, global);

            order.Complete(actor);

            if (actor.Net is not Server server)
                return null;
            if (!product.Product.IsSpawned) // HACK for when the product is one of the ingredients (already existing on top of the workstation)
            {
                product.Product.Global = global + Vector3.UnitZ;
                product.Product.SyncInstantiate(server);
                actor.Map.SyncSpawn(product.Product, global.Above(), Vector3.Zero);
            }
            return product.Product;
        }

        protected override void AddSaveData(SaveTag tag)
        {
            tag.Add(this.OrderID.Save("OrderID"));
            tag.Add(this.Progress.Save("CraftProgress"));
            tag.Add(this.PlacedObjects.SaveNewBEST("PlacedItems"));
        }
        public override void LoadData(SaveTag tag)
        {
            tag.TryGetTagValue("OrderID", out this.OrderID);
            this.Progress = new Progress(tag["CraftProgress"]);
            this.PlacedObjects.TryLoadMutable(tag, "PlacedItems");
        }
        protected override void WriteExtra(BinaryWriter w)
        {
            w.Write(this.OrderID);
            this.Progress.Write(w);
            this.PlacedObjects.Write(w);
        }
        protected override void ReadExtra(BinaryReader r)
        {
            this.OrderID = r.ReadInt32();
            this.Progress = new Progress(r);
            this.PlacedObjects.ReadMutable(r);
        }

        public override void OnUpdate(Actor a, TargetArgs t)
        {
            throw new NotImplementedException();
        }
    }
}
