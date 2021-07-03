﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Start_a_Town_.Net;
using Start_a_Town_.Crafting;

namespace Start_a_Town_
{
    class InteractionCraftVisitorRequest : InteractionPerpetual
    {
        int OrderID;
        int ShopID;
        Progress Progress = new();
        readonly List<ObjectRefIDsAmount> PlacedObjects = new();
        Dictionary<string, ObjectRefIDsAmount> IngredientsUsed = new();
        public InteractionCraftVisitorRequest(int orderID, List<ObjectAmount> placedObjects)
              : this()
        {
            this.OrderID = orderID;
            this.PlacedObjects = placedObjects.Select(o=>new ObjectRefIDsAmount(o.Object, o.Amount)).ToList();
        }
        public InteractionCraftVisitorRequest(int shopid, int orderID, List<ObjectRefIDsAmount> placedObjects)
            : this()
        {
            this.ShopID = shopid;
            this.OrderID = orderID;
            this.PlacedObjects = placedObjects;
        }
        public InteractionCraftVisitorRequest()
            : base("Produce")
        {

        }

        public InteractionCraftVisitorRequest(Workplace shop, CraftOrderNew order, Dictionary<string, ObjectRefIDsAmount> ingredientsUsed) : this()
        {
            this.ShopID = shop.ID;
            this.OrderID = order.ID;
            this.IngredientsUsed = ingredientsUsed;
        }

        public override void Start(GameObject a, TargetArgs t)
        {
            base.Start(a, t);
        }
        public override void Perform(GameObject a, TargetArgs t)
        {
            if (this.Progress.Value < this.Progress.Max)
                return;

            CraftOrderNew order = a.Town.GetShop(this.ShopID).GetOrder(this.OrderID);

            var product = ProduceWithMaterialsOnTopNew(a, t.Global, order);

            if (a.Net is Net.Server)
            {
                var task = a.CurrentTask;
                if (task != null)
                    task.Product = new TargetArgs(product);
            }
            this.Finish(a, t);
        }
        internal override void OnToolContact(GameObject a, TargetArgs t)
        {
            this.Progress.Value += 25;
        }
       
        public override object Clone()
        {
            return new InteractionCraftNew(this.OrderID, this.PlacedObjects);
        }

        GameObject ProduceWithMaterialsOnTopNew(GameObject actor, Vector3 global, CraftOrderNew order)
        {
            var ingr = this.IngredientsUsed.ToDictionary(vk => vk.Key, vk => new ObjectAmount(actor.Net.GetNetworkObject(vk.Value.Object), vk.Value.Amount));
            var reaction = order.Reaction;
            var product = reaction.Products.First().Make(actor as Actor, reaction, ingr);
            product.ConsumeMaterials();
            actor.Map.GetBlockEntity(global)?.GetComp<EntityCompRefuelable>()?.ConsumePower(actor.Map, order.Reaction.Fuel);

            actor.Net.Map.EventOccured(Components.Message.Types.CraftingComplete, actor, global);

            order.Complete(actor);

            if (actor.Net is not Server server)
                return null;
            actor.CurrentTask?.AddCraftedItem(product.Product as Entity);
            product.Product.Global = global + Vector3.UnitZ;
            server.SyncInstantiate(product.Product);
            actor.Map.SyncSpawn(product.Product, global.Above(), Vector3.Zero);

            return product.Product;
        }
        


        protected override void AddSaveData(SaveTag tag)
        {
            tag.Add(this.ShopID.Save("ShopID"));
            tag.Add(this.OrderID.Save("OrderID"));
            tag.Add(this.Progress.Save("CraftProgress"));
            tag.Add(this.PlacedObjects.SaveNewBEST("PlacedItems"));
        }
        public override void LoadData(SaveTag tag)
        {
            tag.TryGetTagValue("ShopID", out this.ShopID);
            tag.TryGetTagValue("OrderID", out this.OrderID);
            this.Progress = new Progress(tag["CraftProgress"]);
            this.PlacedObjects.TryLoadMutable(tag, "PlacedItems");
        }
        protected override void WriteExtra(System.IO.BinaryWriter w)
        {
            w.Write(this.ShopID);
            w.Write(this.OrderID);
            this.Progress.Write(w);
            this.PlacedObjects.Write(w);
            this.IngredientsUsed.WriteNew(w, k => w.Write(k), v => v.Write(w));
        }
        protected override void ReadExtra(System.IO.BinaryReader r)
        {
     
            this.ShopID = r.ReadInt32();
            this.OrderID = r.ReadInt32();
            this.Progress = new Progress(r);
            this.PlacedObjects.ReadMutable(r);
            this.IngredientsUsed.ReadNew(r, r => r.ReadString(), r => new ObjectRefIDsAmount().Read(r) as ObjectRefIDsAmount);
        }

        public override void OnUpdate(GameObject a, TargetArgs t)
        {
            throw new NotImplementedException();
        }
    }
}
