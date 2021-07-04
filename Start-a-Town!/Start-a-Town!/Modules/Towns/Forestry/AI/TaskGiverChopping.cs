using System.Linq;

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
