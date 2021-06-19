using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_
{
    class TaskGiverSellOverCounter : TaskGiver
    {
        protected override AITask TryAssignTask(Actor actor)
        {
            var shops = actor.Town.ShopManager.GetShops().OfType<Shop>();

            var prefs = actor.GetItemPreferences();
            var junk = prefs.GetJunk();
            foreach (var item in junk)
            {
                foreach (var shop in shops)
                {
                    //if (!actor.Inventory.GetItems().Contains(item))
                    //{
                    //    prefs.RemoveJunk(item);
                    //    continue;
                    //}

                    // TODO find a shop worker to sell to
                    // TODO only return a task if there's an available buyer with enough money?
                    // TODO try to sell anyway and drop town rating if unsuccessful?

                    //var itemcost = item.GetValueTotal();
                    //var worker = actor.Map.Town.GetAgents().FirstOrDefault(a => a.HasMoney(itemcost));
                    //if (worker == null)
                    //    continue;
                    //var shop = worker.GetShop();
                    if (!shop.IsValid())
                        continue;
                    if (actor.GetVisitorProperties().IsBlacklisted(shop))
                        continue;
                    

                    var cost = item.GetValueTotal();

                    if (!shop.RequestTransactionSell(actor, item, cost, out var transaction))
                        continue;
                    //worker.InitiateTrade(actor, item, itemcost);
                    //return new AITask(typeof(TaskBehaviorAcceptBuyHandToHand), new TargetArgs(item), new TargetArgs(worker));
                    return new AITask(typeof(TaskBehaviorSellOverCounter), item) { ShopID = shop.ID, Transaction = transaction };
                }
            }
            return null;
        }
    }
}
