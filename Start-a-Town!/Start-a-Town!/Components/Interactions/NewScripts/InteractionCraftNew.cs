﻿using System;
using System.Collections.Generic;
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
        int BaseWorkAmount = 5;//25;
        int GetWorkAmount(Actor actor)
        {
            var work = BaseWorkAmount;
            var tool = actor.GetEquipmentSlot(GearType.Types.Mainhand);
            if (tool is null)
                return work;
            return work;
        }
        static readonly TaskConditions conds =
                new TaskConditions(
                    new AllCheck(
                        //new RangeCheck(1),
                        RangeCheck.Sqrt2
                        //,
                        //new MaterialsPresent()
                        ));

        public override TaskConditions Conditions
        {
            get
            {
                return conds;
            }
        }

        public override void Start(GameObject a, TargetArgs t)
        {
            base.Start(a, t);
        }
        public override void Perform(GameObject a, TargetArgs t)
        {
            if (this.Progress.Value >= this.Progress.Max)
            {
                var orderContainer = a.Map.GetBlockEntity(t.Global).GetComp<BlockEntityCompWorkstation>();
                var order = orderContainer.GetOrder(this.OrderID);

                var product = ProduceWithMaterialsOnTopNew(a, t.Global, order);

                //if (order.Reaction.Fuel)
                //    block.ConsumeFuel(a.Map, t.Global);

                if (a.Net is Net.Server)
                {
                    var task = a.CurrentTask;
                    if (task != null)
                        task.Product = new TargetArgs(product);
                }
                this.Finish(a, t);
            }
        }
        internal override void OnToolContact(GameObject a, TargetArgs t)
        {
            var work = StatDefOf.ToolEfficiency.GetValue(a);
            this.Progress.Value += work * BaseWorkAmount;// 25;
        }
        
        
        public override object Clone()
        {
            return new InteractionCraftNew(this.OrderID, this.PlacedObjects);

        }

        GameObject ProduceWithMaterialsOnTopNew(GameObject a, Vector3 global, CraftOrderNew order)
        {
            var actor = a as Actor;
            //var ingredients = this.Reagents;
            //var ingr = actor.CurrentTask.PlacedObjects;
            var ingr = this.PlacedObjects.Select(o => new ObjectAmount(actor.NetNew.GetNetworkObject(o.Object), o.Amount)).ToList();
            
            var reaction = order.Reaction;
            //var product = reaction.Products.First().GetProduct(reaction, ingredients);
            var product = reaction.Products.First().Make(actor, reaction, ingr);//, out var sortedIngredients);
            //var contents = this.PlacedItemsRefs.Select(r => actor.Net.GetNetworkObject(r) as Item);
            var skillAwardAmount = 100;
            actor.AwardSkillXP(reaction.CraftSkill, skillAwardAmount);


            product.ConsumeMaterials();
            //if (!product.MaterialsAvailable(contents))
            //    throw new Exception(); //return;
            actor.Map.GetBlockEntity(global)?.GetComp<EntityCompRefuelable>()?.ConsumePower(actor.Map, order.Reaction.Fuel);

            actor.Net.Map.EventOccured(Components.Message.Types.CraftingComplete, actor, global);

            order.Complete(actor);


            if (actor.Net is not Server server)
                return null;
            if (!product.Product.IsSpawned) // HACK for when the product is one of the ingredients (already existing on top of the workstation)
            {
                product.Product.Global = global + Vector3.UnitZ;
                server.SyncInstantiate(product.Product);
                actor.Map.SyncSpawn(product.Product, global.Above(), Vector3.Zero);
                //server.SyncSpawn(product.Product, actor.Map, global + Vector3.UnitZ);
            }
            //product.Product.Spawn(actor.Map, global + Vector3.UnitZ);
            return product.Product;
        }
        


        protected override void AddSaveData(SaveTag tag)
        {
            tag.Add(this.OrderID.Save("OrderID"));
            tag.Add(this.Progress.Save("CraftProgress"));
            //tag.Add(this.PlacedItemsRefs.Save("MaterialRefs"));
            tag.Add(this.PlacedObjects.SaveNewBEST("PlacedItems"));
        }
        public override void LoadData(SaveTag tag)
        {
            tag.TryGetTagValue("OrderID", out this.OrderID);
            this.Progress = new Progress(tag["CraftProgress"]);
            //this.PlacedItemsRefs.Load(tag, "MaterialRefs");
            this.PlacedObjects.TryLoadMutable(tag, "PlacedItems");
        }
        protected override void WriteExtra(System.IO.BinaryWriter w)
        {

            w.Write(this.OrderID);
            //w.Write(this.Ingredients.Count);
            //foreach(var kv in this.Ingredients)
            //{
            //    w.Write(kv.Key);
            //    w.Write(kv.Value);
            //}
            this.Progress.Write(w);
            this.PlacedObjects.Write(w);
            //w.Write(this.PlacedItemsRefs);
        }
        protected override void ReadExtra(System.IO.BinaryReader r)
        {
       
            this.OrderID = r.ReadInt32();

            //var count = r.ReadInt32();
            //for (int i = 0; i < count; i++)
            //{
            //    this.Ingredients.Add(r.ReadString(), r.ReadInt32());
            //}
            this.Progress = new Progress(r);
            this.PlacedObjects.ReadMutable(r);
            //this.PlacedItemsRefs = r.ReadListInt();
        }

        public override void OnUpdate(GameObject a, TargetArgs t)
        {
            throw new NotImplementedException();
        }
    }
    

}
