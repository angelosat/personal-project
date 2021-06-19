using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_
{
    class TaskGiverSell : TaskGiver
    {
        protected override AITask TryAssignTask(Actor actor)
        {
            var prefs = actor.GetItemPreferences();
            var junk = prefs.GetJunk();
            foreach (var item in junk)
            {
                //if (!actor.Inventory.GetItems().Contains(item))
                //{
                //    prefs.RemoveJunk(item);
                //    continue;
                //}

                // TODO find a shop worker to sell to
                // TODO only return a task if there's an available buyer with enough money?
                // TODO try to sell anyway and drop town rating if unsuccessful?
                var itemcost = item.GetValueTotal();
                var worker = actor.Map.Town.GetAgents().FirstOrDefault(a => a.HasMoney(itemcost));
                if (worker == null)
                    continue;
                worker.InitiateTrade(actor, item, itemcost);
                //return new AITask(typeof(TaskBehaviorAcceptBuyHandToHand), new TargetArgs(item), new TargetArgs(worker));
                return new AITask(typeof(TaskBehaviorSell), new TargetArgs(item), new TargetArgs(worker)) { AmountA = item.StackSize };
            }
            return null;
        }
    }
}
