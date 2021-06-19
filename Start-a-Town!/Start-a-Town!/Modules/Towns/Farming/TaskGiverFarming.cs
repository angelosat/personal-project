using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.AI;
using Start_a_Town_.AI.Behaviors;
using Start_a_Town_.Towns.Forestry;
using Start_a_Town_.Components;
using Start_a_Town_.Components.Skills;

namespace Start_a_Town_
{
    class TaskGiverFarming : TaskGiver
    {
        protected override AITask TryAssignTask(Actor actor)
        {
            if (!actor.HasLabor(JobDefOf.Lumberjack))
                return null;
            var manager = actor.Map.Town.FarmingManager;
            var trees = manager.GetChoppableTrees().Where(o => actor.CanReserve(o)).OrderByReachableRegionDistance(actor);// .OrderByDistanceTo(actor);
            if (!trees.Any())
                return null;
            var task = new AITask(typeof(TaskBehaviorChoppingNew));// AITaskChoppingNew();
            var equipped = actor.GetEquipmentSlot(GearType.Mainhand);
            if (!(equipped != null && equipped.ProvidesSkill(ToolAbilityDef.Chopping)))
                task.Tool = TaskHelper.FindItemAnywhere(actor, o => o.ProvidesSkill(ToolAbilityDef.Chopping));
            task.TargetA = new TargetArgs(trees.First());
            return task;
        }
    }
}
