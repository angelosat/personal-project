using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_
{
    class TaskGiverBuy : TaskGiver
    {
        protected override AITask TryAssignTask(Actor actor)
        {
            // check available money
            var money = actor.CountItemsInInventory(ItemDefOf.Coins);
            if (money == 0)
                return null;

            // find things for sale
            var shops = actor.Town.ShopManager.GetShops().OfType<Shop>();
            foreach(var shop in shops)
            {
                //var item = shop.GetItems().FirstOrDefault(o => IsForSale(o) && money >= o.GetValueTotal());
                //if (item == null)
                //    continue;

                var items = shop.GetItems(o => IsForSale(o) && money >= o.GetValueTotal());
                var item = DecideItem(actor, items);
                if (item == null)
                    continue;

                var worker = shop.GetWorkers().FirstOrDefault();
                if (worker == null)
                    continue; // TODO maybe attempt to buy an item but fail the transaction and drop town approval rating if there's no worker ?

                //worker.GetState().TradingPartner = actor;
                var cost = item.GetValueTotal();
                if (!worker.InitiateTrade(actor, item, cost))
                    continue;
                return new AITask(typeof(TaskBehaviorBuy), new TargetArgs(item), new TargetArgs(worker));

                //var owner = actor.Net.GetNetworkObject(shop.OwnerID);
                //owner.GetState().TradingPartner = actor;
                //return new AITask(typeof(TaskBehaviorBuy), new TargetArgs(item), new TargetArgs(owner));
            }

            return null;
            ////var item = actor.Map.GetObjects().FirstOrDefault(IsForSale) as Entity; // HACK
            //var item = actor.Map.GetObjects().FirstOrDefault(o => IsForSale(o) && money >= o.GetValueTotal()) as Entity; // HACK
            //if (item == null)
            //    return null;

            //// find item's owner/shop
            ////var shop = Shop.FindShopFromItem(item);
            //var owner = actor.Map.Town.GetAgents().FirstOrDefault(); // HACK


            //owner.GetState().TradingPartner = actor;
            //return new AITask(typeof(TaskBehaviorBuy), new TargetArgs(item), new TargetArgs(owner));
        }

        static private bool IsForSale(GameObject o)
        {
            //if (!o.IsHaulable)
            //    return false;
            if (o.Def.Category == null)
                return false;
            return true;
        }
        static Entity DecideItem(Actor actor, IEnumerable<Entity> items)
        {
            //var scores = items.OrderByDescending(i => actor.EvaluateItem(i));
            var scores = items
                .Select(i => (i, actor.EvaluateItem(i)))
                .Where(p => p.Item2 > 0)
                .OrderByDescending(p => p.Item2)
                .ToList();

            var item = scores.FirstOrDefault();
            return item.Item1;
        }
        //int Score(Actor actor, Entity item)
        //{

        //    return 0;
        //}
    }
}
