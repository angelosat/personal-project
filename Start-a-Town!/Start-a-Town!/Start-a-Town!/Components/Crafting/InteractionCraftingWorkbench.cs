﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.Components.Crafting;
using Start_a_Town_.Components.Interactions;
using Start_a_Town_.Blocks;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.Components.Crafting
{
    class InteractionCraftingWorkbench : Interaction
    {
        Reaction.Product.ProductMaterialPair Product;
        Container MaterialsContainer;
        Vector3 WorkstationGlobal;
        BlockWorkbench.Entity Workbench;

        public InteractionCraftingWorkbench(Reaction.Product.ProductMaterialPair product, BlockWorkbench.Entity entity, Vector3 workstationGlobal)
            : base(
            "Craft",
            1
            //,
            //new TaskConditions(
            //    new AllCheck(
            //        new ScriptTaskCondition("ToolEquipped", (a, t) =>
            //        {
            //            var tool = GearComponent.GetSlot(a, GearType.Mainhand);
            //            return tool.Object.Network.ID == product.Tool.Network.ID;
            //        }),
            //        new ScriptTaskCondition("Materials", this.MaterialsAvailable)
            //    //new RangeCheck(t => t.Global, InteractionOld.DefaultRange),
            //    //new TargetTypeCheck(TargetType.Entity))
            //    )
            //)
            )
        {
            this.Product = product;
            this.WorkstationGlobal = workstationGlobal;
            this.Workbench = entity;
            this.MaterialsContainer = entity.Storage;

            //this.Conditions = new TaskConditions(
            //    new AllCheck(
            //        new ScriptTaskCondition("ToolEquipped", (a, t) =>
            //        {
            //            if (product.Tool == null)
            //                return true;
            //            var tool = GearComponent.GetSlot(a, GearType.Mainhand);
            //            if (tool.Object == null)
            //                return false;

            //            return tool.Object.Network.ID == product.Tool.Network.ID;
            //        }),
            //        new ScriptTaskCondition("Materials", this.MaterialsAvailable)
            //    ));
        }

        public override bool AvailabilityCondition(GameObject actor, TargetArgs target)
        {
            return this.Conditions.GetFailedCondition(actor, target) == null;

            return MaterialsAvailable(actor, target);
        }

        private bool MaterialsAvailable(GameObject actor, TargetArgs target)
        {
            //if (target.Object != null)
            //    return this.Product.MaterialsAvailable(actor, target.Object);
            //else
            //    return this.Product.MaterialsAvailable(actor);


            var contents = this.Workbench.GetReagentSlots(actor); //actor.GetComponent<PersonalInventoryComponent>().GetContents().Concat(this.MaterialsContainer.Slots).ToList();
            return this.Product.MaterialsAvailable(contents);

            return this.Product.MaterialsAvailable(this.MaterialsContainer.Slots);



            //var nearbyMaterials = actor.GetNearbyObjects(rng => rng <= 2).ConvertAll(o => o.ToSlot());
            ////var container = actor.GetComponent<PersonalInventoryComponent>().Children;
            //var container = target.Object.GetChildren();
            ////var availableMats = container;// nearbyMaterials.Concat(container);
            //var availableMats = container.Concat(nearbyMaterials);
            //return this.Product.MaterialsAvailable(availableMats);
        }
        public override void Perform(GameObject actor, TargetArgs target)
        {
            // TODO: check here if crafting is legal (check available materials etc)
            var contents = this.Workbench.GetReagentSlots(actor);// actor.GetComponent<PersonalInventoryComponent>().GetContents().Concat(this.MaterialsContainer.Slots).ToList();

            //if (!this.Product.MaterialsAvailable(this.MaterialsContainer.Slots))
            if (!this.Product.MaterialsAvailable(contents))
                return;
            //this.Product.ConsumeMaterials(actor.Net, this.MaterialsContainer.Slots);
            this.Product.ConsumeMaterials(actor.Net, contents);

            actor.Net.EventOccured(Message.Types.InventoryChanged, target.Object);
            var server = actor.Net as Net.Server;



            if (server == null)
                return;
            this.Product.Product.Global = this.WorkstationGlobal + Vector3.UnitZ;
            server.SyncInstantiate(this.Product.Product);
            server.SyncSpawn(this.Product.Product);
            return;

            server.SyncInstantiate(this.Product.Product);
            HaulComponent.Carry(actor, this.Product.Product);
            server.RemoteProcedureCall(new TargetArgs(actor), Message.Types.Carry, BitConverter.GetBytes(this.Product.Product.InstanceID), actor.Global);
            //server.PopLoot(this.Product.Product, actor);
        }

        public void PerformOld(GameObject actor, TargetArgs target)
        {
            //Instance.PopLoot(product.Product, Instance.NetworkObjects[chid]);

            // TODO: check here if crafting is legal (check available materials etc)
            var nearbyMaterials = actor.GetNearbyObjects(rng => rng <= 2).ConvertAll(o => o.ToSlot());
            //var container = actor.GetComponent<PersonalInventoryComponent>().Children;
            //var container = target.Object.GetChildren();
            var container = this.MaterialsContainer.Slots.ToList();

            //var availableMats = container;// nearbyMaterials.Concat(container);
            var availableMats = container.Concat(nearbyMaterials);
            if (!this.Product.MaterialsAvailable(availableMats))
                return;
            this.Product.ConsumeMaterials(actor.Net, availableMats);
            actor.Net.EventOccured(Message.Types.InventoryChanged, target.Object);
            var server = actor.Net as Net.Server;
            if (server == null)
                return;
            server.SyncInstantiate(this.Product.Product);
            //Instance.PostLocalEvent(msg.Player.Character, Message.Types.Insert, product.Product.ToSlot());
            server.PopLoot(this.Product.Product, actor);
          
            /*

            var benchEntity = target.Object;
            WorkbenchReactionComponent bench = benchEntity.GetComponent<WorkbenchReactionComponent>();
            //var product = comp.SelectedReaction.Products.First().GetProduct(comp.SelectedReaction, Player.Actor, comp.Slots);

            // check materials and consume them if they exist, otherwise return
            foreach (var mat in this.Product.Requirements)
                if (!bench.Slots.Check(mat.ObjectID, mat.Amount))
                    return;
            foreach (var mat in this.Product.Requirements)
                bench.Slots.Consume(mat.ObjectID, mat.Max);

            bench.Craft(benchEntity, this.Product);
             */
        }

        public override object Clone()
        {
            return new InteractionCraftingWorkbench(this.Product, this.Workbench, this.WorkstationGlobal);
        }
    }
}
