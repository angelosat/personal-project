using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.AI.Behaviors;

namespace Start_a_Town_
{
    class TaskGiverEquipPreferredItems : TaskGiver
    {
        protected override AITask TryAssignTask(Actor actor)
        {
            //var props = actor.Map.World.Population.GetVisitorProperties(actor);
            //var prefs = props.ItemPreferences;
            var prefs = actor.GetItemPreferences();
            foreach(var gt in actor.GetGearTypes())
            {
                var item = prefs.GetPreference(gt);
                var equipped = actor.GetEquipmentSlot(gt);
                if (equipped == item)
                    continue;
                else
                {
                    return new AITask(typeof(BehaviorEquipItemNew), new TargetArgs(item));
                    // TODO check world incase the item is available in the map but not inside inveotry? return a pickup task in that case?
                    // TODO return equipping taskbehavior
                    // add previously equipped to todiscard?
                    //return new AITask(typeof(taskbehaviorequi))
                }
            }
            return null;
        }
    }
}
