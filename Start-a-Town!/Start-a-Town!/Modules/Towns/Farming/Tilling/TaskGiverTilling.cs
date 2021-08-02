using System.Linq;
using Start_a_Town_.Towns.Farming;

namespace Start_a_Town_
{
    class TaskGiverTilling : TaskGiver
    {
        protected override AITask TryAssignTask(Actor actor)
        {
            if (!actor.HasLabor(JobDefOf.Farmer))
                return null;
            var loc = GetBestTillingLocation(actor);
            if (!loc.HasValue)
                return null;
            var task = new AITask(typeof(TaskBehaviorTillingNew));
            task.SetTarget(TaskBehaviorTillingNew.TargetInd, new TargetArgs(actor.Map, loc.Value));
            FindTool(actor, task, ToolAbilityDef.Argiculture);
            return task;
        }

        static IntVec3? GetBestTillingLocation(Actor actor)
        {
            var map = actor.Map;
            var reachableZones = map.Town.ZoneManager
                .GetZones<GrowingZone>()
                .Where(z => z.Tilling && z.GetTillingPositions() is var tilling && tilling.Any() && actor.CanReachNew(tilling.First()));
            var locs = reachableZones
                .SelectMany(z => z.GetTillingPositions())
                .Where(pos => actor.CanReserve(pos));
            if (!locs.Any())
                return null;
            return locs.First();
        }
    }
}
