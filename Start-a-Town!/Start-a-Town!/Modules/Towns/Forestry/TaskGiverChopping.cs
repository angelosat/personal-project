using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.AI.Behaviors;
using Start_a_Town_.Components;
using Start_a_Town_.Components.Skills;
using Start_a_Town_.AI.Behaviors.ItemOwnership;
using Start_a_Town_.AI;
using Microsoft.Xna.Framework;

namespace Start_a_Town_
{
    class TaskGiverChopping : TaskGiver
    {
        protected override AITask TryAssignTask(Actor actor)
        {
            if (!actor.HasLabor(JobDefOf.Lumberjack))
                return null;
            var manager = actor.Map.Town.ChoppingManager;
            var trees = manager.GetTrees()
                .Where(o => actor.CanReserve(o))
                .OrderByReachableRegionDistance(actor);
            if (!trees.Any())
                return TaskHelper.TryStoreEquipped(actor, GearType.Mainhand);

            var task = new AITask(typeof(TaskBehaviorChoppingNew));
            FindTool(actor, task, ToolAbilityDef.Chopping);
            task.TargetA = new TargetArgs(trees.First());
            return task;
        }
    }
}
