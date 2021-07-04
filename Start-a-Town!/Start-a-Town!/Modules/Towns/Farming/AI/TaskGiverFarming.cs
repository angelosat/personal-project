using System.Linq;

namespace Start_a_Town_
{
    class TaskGiverFarming : TaskGiver
    {
        protected override AITask TryAssignTask(Actor actor)
        {
            if (!actor.HasLabor(JobDefOf.Lumberjack))
                return null;
            var manager = actor.Map.Town.FarmingManager;
            var trees = manager.GetChoppableTrees().Where(o => actor.CanReserve(o)).OrderByReachableRegionDistance(actor);
            if (!trees.Any())
                return null;
            var task = new AITask(typeof(TaskBehaviorChoppingNew));
            var equipped = actor.GetEquipmentSlot(GearType.Mainhand);
            if (!(equipped != null && equipped.ProvidesSkill(ToolAbilityDef.Chopping)))
                task.Tool = TaskHelper.FindItemAnywhere(actor, o => o.ProvidesSkill(ToolAbilityDef.Chopping));
            task.TargetA = new TargetArgs(trees.First());
            return task;
        }
    }
}
