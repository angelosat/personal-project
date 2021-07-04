namespace Start_a_Town_
{
    class TaskGiverHarvestingNew : TaskGiver
    {
        protected override AITask TryAssignTask(Actor actor)
        {
            if (!actor.HasLabor(JobDefOf.Harvester))
                return null;
            var map = actor.Map;
            var zones = map.Town.ZoneManager.GetZones<GrowingZone>();
            foreach (var zone in zones)
            {
                var plants = zone.GetHarvestablePlantsLazy();
                foreach (var plant in plants)
                {
                    if (!actor.CanReserve(plant) ||
                        !actor.CanReach(plant))
                        continue;
                    var task = new AITask(TaskDefOf.Harvesting, new TargetArgs(plant));
                    return task;
                }
            }
            return null;
        }
    }
}
