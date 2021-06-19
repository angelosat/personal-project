using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.Components;

namespace Start_a_Town_.AI.Behaviors.ItemOwnership
{
    class TaskGiverDropItem : TaskGiver
    {
        protected override AITask TryAssignTask(Actor actor)
        {
            var carriedItems = actor.InventoryAll();// PersonalInventoryComponent.GetAllItems(actor);
            var possesions = actor.GetPossesions();// NpcComponent.GetPossesions(actor);
            var notOwnedItems = carriedItems.Except(possesions);
            var itemToDrop = notOwnedItems.FirstOrDefault();
            if (itemToDrop == null)
                return null;
            return new AITask(typeof(TaskBehaviorDropItem)) { TargetA = new TargetArgs(itemToDrop) };// TaskDropItem(itemToDrop);
        }
    }
}
