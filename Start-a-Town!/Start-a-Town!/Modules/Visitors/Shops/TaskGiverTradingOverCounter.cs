using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_
{
    class TaskGiverTradingOverCounter : TaskGiver
    {
        protected override AITask TryAssignTask(Actor actor)
        {
            var shop = actor.Town.ShopManager.GetShop<Shop>(actor);
            if (shop == null)
                return null;
            if (!shop.IsValid())
                return null;
            if (!shop.TryGetNextTransaction(out var transaction))
                return null;
            if (!shop.CanExecuteTransaction(actor, transaction))
                return null;
            if (transaction.Type == Transaction.Types.Buy)
                return new AITask(typeof(TaskBehaviorAcceptSellOverCounter), (actor.Map, shop.Counter.Value));
            else if (transaction.Type == Transaction.Types.Sell)
                return new AITask(typeof(TaskBehaviorAcceptBuyOverCounter)) { ShopID = shop.ID, Transaction = transaction }; // shop holds value for counter so no need to pass it to the task as a target
            else
                throw new Exception();
        }
        //protected override AITask TryAssignTask(Actor actor)
        //{
        //    var shop = actor.Town.ShopManager.FindShop(actor);
        //    if (shop == null)
        //        return null;
        //    var customer = shop.GetNextCustomer();
        //    if (customer == null)
        //        return null;
        //    if (!shop.Counter.HasValue)
        //        throw new Exception();
        //    var partnerbhav = customer.CurrentTask.BehaviorType;
        //    if (partnerbhav == typeof(TaskBehaviorBuyOverCounter))
        //        return new AITask(typeof(TaskBehaviorAcceptSellOverCounter), (actor.Map, shop.Counter.Value));
        //    else if (partnerbhav == typeof(TaskBehaviorSellOverCounter))
        //    {
        //        var transaction = shop.GetTransaction(customer);
        //        //return new AITask(typeof(TaskBehaviorAcceptBuyOverCounter), (actor.Map, shop.Counter.Value)) { Shop = shop, Transaction = transaction };
        //        return new AITask(typeof(TaskBehaviorAcceptBuyOverCounter)) { Shop = shop, Transaction = transaction }; // shop holds value for counter so no need to pass it to the task as a target

        //    }
        //    else
        //        throw new Exception();
        //}
    }
}
