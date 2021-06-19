﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.AI;
using Start_a_Town_.AI.Behaviors;

namespace Start_a_Town_
{
    class TaskBehaviorAcceptSellOverCounter : BehaviorPerformTask
    {
        protected override IEnumerable<Behavior> GetSteps()
        {
            // TODO wait for price negotiation
            // TODO send reply / complete transaction
            var actor = this.Actor;
            var state = this.Actor.GetState();
            //var shop = actor.Workplace as Shop;
            var task = this.Task;
            var shop = actor.Town.GetShop(task.ShopID) as Shop;

            yield return new BehaviorCustom()
            {
                InitAction = () =>
                {
                    task.SetTarget(TargetIndex.A, shop.Counter.Value);
                    task.SetTarget(TargetIndex.B, shop.GetNextSaleItem());
                    //task.SetTarget(TargetIndex.B, shop.)
                }
            };
            yield return new BehaviorGetAtNewNew(TargetIndex.A);
            yield return new BehaviorWait(() => task.TargetB.Object.Parent == null && task.TargetB.Object.Global.SnapToBlock() == task.TargetA.Global.Above());
            //yield return BehaviorHaulHelper.StartCarrying(TargetIndex.B);
            yield return new BehaviorInteractionNew(TargetIndex.B, () => new InteractionHaul());
            // TODO wait until money on counter
            yield return new BehaviorWait(() =>
            {
                var itemOnCounter = actor.Map.GetObjects(task.TargetA.Global.Above());
                var money = itemOnCounter.FirstOrDefault(i => i.Def == ItemDefOf.Coins);
                if (money == null)
                    return false;
                task.SetTarget(TargetIndex.C, money);
                return true;
            });
            yield return new BehaviorInteractionNew(() => task.TargetC, () => new InteractionSwapCarried());

            // if carrying coins, store in inventory. otherwise drop or haul to stockpile
            yield return new BehaviorInteractionNew(TargetIndex.A, () => //this.Actor.Carried.Def == ItemDefOf.Coins ? new InteractionStoreHauled() : new InteractionThrow());
            {
                if (this.Actor.Carried.Def == ItemDefOf.Coins)
                    return new InteractionStoreHauled();
                else
                    return new InteractionThrow();
            });
            // UNDONE i remove customer on the buyer's behavior finish action
            //yield return new BehaviorWait(() =>
            //{
            //    var item = task.TargetB.Object as Entity;
            //    var customer = shop.GetNextCustomer();
            //    if (item.Parent == customer)
            //    {
            //        shop.RemoveCustomer(customer);
            //        return true;
            //    }
            //    return false;
            //});
        }
    }
}
