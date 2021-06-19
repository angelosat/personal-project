﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.AI;
using Start_a_Town_.AI.Behaviors;

namespace Start_a_Town_
{
    class TaskBehaviorBuyOverCounter : BehaviorPerformTask
    {
        static int Patience = Engine.TicksPerSecond * 5; // TODO get patience value from actor
        protected override IEnumerable<Behavior> GetSteps()
        {
            var actor = this.Actor;
            var task = this.Task;
            //this.FailOn(() => // TODO refactor this
            //{
            //    if (this.Task.TicksWaited > Patience)
            //    {
            //        actor.GetVisitorProperties().BlacklistShop(task.ShopID);
            //        return true;
            //    }
            //    return false;
            //});
            this.FailOnRanOutOfPatienceWaiting(() => actor.GetVisitorProperties().BlacklistShop(task.ShopID));
            var item = task.TargetA.Object as Entity;

            this.AddFinishAction(() => actor.Town.ShopManager.GetShop<Shop>(task.ShopID).RemoveCustomer(actor));
            var counter = task.TargetB.Global;
            yield return new BehaviorGetAtNewNew(TargetIndex.A);
            yield return BehaviorHaulHelper.StartCarrying(TargetIndex.A);
            yield return BehaviorReserve.Reserve(TargetIndex.A); // reserve item so noone picks it up after placing it on the shop counter
            // TODO start checking if the shop has a worker
            // if no worker or all workers busy, wait a bit and then cancel the behavior and drop town approval rating
            yield return new BehaviorGetAtNewNew(TargetIndex.B);
            
           
            yield return new BehaviorInteractionNew(() => (actor.Map, counter.Above()), () => new UseHauledOnTarget());
            //        // TODO shop worker reserves dropped item immediately

            

            yield return new BehaviorWait(() => {
                // wait until a shop worker has picked the item up and is in their carry slot
                var worker = item.Parent; // HACK this will just wait for the first person to pick up the item. check if it's a worker of the current shop
                if (worker == null)
                    return false;
                if (worker.Carried != item)
                    return false; // TODO maybe throw exception?
                //task.SetTarget(TargetIndex.C, worker);
                return true;
                });
            // WARNING if coins in inventory have somehow been reduced below the item's cost since starting the behavior, cancel everything
            yield return new BehaviorCustom()
            {
                InitAction = () =>
                {
                    var item = this.Task.TargetA.Object as Entity;
                    this.Task.SetTarget(TargetIndex.C, this.Actor.InventoryFirst(i => i.Def == ItemDefOf.Coins));
                    var totalvalue = item.GetValueTotal();
                    if (totalvalue <= 0)
                        throw new Exception();
                    this.Task.SetAmount(TargetIndex.C, totalvalue);
                }
            };
            yield return new BehaviorInteractionNew(TargetIndex.C, () => new InteractionHaul(this.Task.AmountC));
            yield return new BehaviorCustom() { InitAction = () => actor.Reserve(actor.Carried) };
            //yield return new BehaviorInteractionNew(TargetIndex.B, () => new InteractionGiveItem(true));
            yield return new BehaviorInteractionNew(() => (actor.Map, counter.Above()), () => new UseHauledOnTarget());
            // TODO wait for the item to be placed ontop of the counter, and then pick it up
            yield return new BehaviorWait(() => item.Parent == null && item.Global.SnapToBlock() == counter.Above());
            //yield return BehaviorHaulHelper.StartCarrying(TargetIndex.A);
            yield return new BehaviorInteractionNew(TargetIndex.A, () => new InteractionHaul());

            yield return new BehaviorInteractionNew(() => new InteractionStoreHauled());
            //yield return new BehaviorCustom()
            //{
            //    InitAction = () =>
            //    {
            //        //var target = this.Task.TargetB.Object as Actor;
            //        //target.GetState().TradingPartner = null;
            //        var worker = task.TargetC.Object as Actor;
            //        var shop = worker.Map.Town.ShopManager.FindShop(worker);
            //        shop.RemoveCustomer(actor);
            //        //shop.RemoveSale(item);
            //    }
            //};
            // TODO behavior negotiate price
            // TODO behavior wait for reply
            // TODO complete transaction
            // TODO insert item to inventory
        }

        protected override bool InitExtraReservations()
        {
            return
                this.Actor.ReserveAsManyAsPossible(this.Task.TargetA, this.Task.TargetA.Object.StackSize);
        }
    }
}
