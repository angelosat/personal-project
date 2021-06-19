using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.AI;
using Start_a_Town_.AI.Behaviors;

namespace Start_a_Town_
{
    class TaskBehaviorSellOverCounter : BehaviorPerformTask
    {
        const TargetIndex Item = TargetIndex.A;
        const TargetIndex Counter = TargetIndex.B;
        const TargetIndex Money = TargetIndex.C;
        static int Patience = Engine.TicksPerSecond * 5; // TODO get patience value from actor

        protected override IEnumerable<Behavior> GetSteps()
        {
            var actor = this.Actor;
            var state = this.Actor.GetState();
            var task = this.Task;
            var shop = actor.Town.ShopManager.GetShop<Shop>(task.ShopID);
            var transaction = task.Transaction;
            var customer = transaction.Customer;
            var item = actor.Net.GetNetworkObject(transaction.Item);
            var cost = transaction.Cost;
            var counter = shop.Counter.Value;
            var counterSurface = counter.Above();

            this.FailOn(() => !shop.IsValid());
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

            this.AddFinishAction(() => shop.RemoveCustomer(actor));

            yield return BehaviorHelper.SetTarget(Counter, counter);
            yield return BehaviorHelper.MoveTo(Counter);
            yield return BehaviorHelper.SetTarget(Item, item);
            yield return BehaviorHelper.CarryFromInventory(Item);
            yield return BehaviorReserve.Reserve(Item);
            //yield return BehaviorHelper.WaitForItem(Item, counterSurface, o => o.Def == ItemDefOf.Coins && o.StackSize >= cost);
            //yield return new BehaviorInteractionNew(Money, () => new InteractionSwapCarried());
            // TODO place item on counter first? or wait for money on counter and then swap?
            // WTF if i place item on counter first, then i'll have to retrieve it in case of failure
            yield return new BehaviorInteractionNew(() => (actor.Map, counterSurface), () => new UseHauledOnTarget());
            yield return BehaviorHelper.WaitForItem(Money, counterSurface, o => o.Def == ItemDefOf.Coins && o.StackSize >= cost);
            yield return new BehaviorInteractionNew(Money, () => new InteractionHaul());

            //yield return new BehaviorWait(() =>
            //{
            //    var money = actor.Map.GetObjects(counterSurface).FirstOrDefault(o => o.Def == ItemDefOf.Coins);
            //    if (money == null)
            //        return false;
            //    if (money.StackSize < cost)
            //        return false; // TODO completely fail behavior instead of continuing to wait?
            //    task.SetTarget(Money, money);
            //    return true;
            //}); // TODO retrieve item if behavior fails while waiting (if going with placing the item on the counter before the money)
            yield return new BehaviorInteractionNew(() => new InteractionStoreHauled());
        }
    }
}
