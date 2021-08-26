using System;
using System.Collections.Generic;
using System.Linq;

namespace Start_a_Town_
{
    class TaskGiverPlanting : TaskGiver
    {
        protected override AITask TryAssignTask(Actor actor)
        {
            var map = actor.Map;
            if (!actor.HasJob(JobDefOf.Farmer))
                return null;
            // TODO: iterate through all zones until one with an available seed type is found

            var zones = map.Town.ZoneManager.GetZones<GrowingZone>();
            foreach (var zone in zones)
            {
                var plant = zone.Plant;
                if (plant == null)
                    continue;
                if (!zone.Planting)
                    continue;
                var allLocs = zone.GetSowingPositions();
                if (!allLocs.Any())
                    continue;
                if (!actor.CanReachNew(allLocs.First()))
                    continue;

                var allSowablePositions = allLocs.Where(g => actor.CanReserve(g));
                if (!allSowablePositions.Any())
                    continue;

                var allseeds = actor.Map.GetObjectsLazy().Where(c => c.IsSeedFor(plant));// && actor.CanReserve(c));//
                                                                                                                //).OrderByReachableRegionDistance(actor);
                var encumberanceLimit = actor.MaxCarryable(ItemDefOf.Seeds);

                var (sources, destinations) = Distribute(actor, encumberanceLimit, allseeds, allSowablePositions.Select(p => new TargetArgs(actor.Map, p)), t => 1);

                if(!sources.Any() || !destinations.Any())
                    continue;

                var task = new AITask(TaskDefOf.Sowing);// typeof(TaskBehaviorDeliverMaterials));
                task.AddTargets(TaskBehaviorDeliverMaterials.MaterialID, sources);
                task.AddTargets(TaskBehaviorDeliverMaterials.DestinationID, destinations);

                return task;
            }
            return null;
        }
        (List<(TargetArgs source, int amount)> sources, List<(TargetArgs destination, int amount)> destinations) Distribute(Actor actor, int maxAmount, IEnumerable<GameObject> sources, IEnumerable<TargetArgs> destinations, Func<TargetArgs, int> targetAmountGetter)
        {
            List<(TargetArgs source, int amount)> sourcesAmounts = new();
            List<(TargetArgs destination, int amount)> destinationsAmounts = new();

            var enumSources = sources.GetEnumerator();
            var enumTargets = destinations.GetEnumerator();
            var remaining = maxAmount;
            while (enumSources.MoveNext() && remaining > 0)
            {
                var count = 0;
                var currentSource = enumSources.Current;
                var unreservedAmount = actor.GetUnreservedAmount(currentSource);
                var carryFromCurrent = Math.Min(remaining, unreservedAmount);// currentSource.StackSize);
                while (enumTargets.MoveNext() && carryFromCurrent > 0)
                {
                    var currentDest = enumTargets.Current;
                    var idealAmountToDistribute = targetAmountGetter(currentDest);
                    var actualAmountToDistribute = Math.Min(idealAmountToDistribute, carryFromCurrent);
                    carryFromCurrent -= actualAmountToDistribute;
                    remaining -= actualAmountToDistribute;
                    count += actualAmountToDistribute;
                    destinationsAmounts.Add((currentDest, actualAmountToDistribute));
                }
                sourcesAmounts.Add((currentSource, count));
            }
            return (sourcesAmounts, destinationsAmounts);
        }

        //protected override AITask TryAssignTask(Actor actor)
        //{
        //    var map = actor.Map;
        //    if (!actor.HasLabor(JobDefOf.Farmer))
        //        return null;
        //    // TODO: iterate through all zones until one with an available seed type is found

        //    var zones = map.Town.ZoneManager.GetZones<GrowingZone>();
        //    foreach (var zone in zones)
        //    {
        //        var plant = zone.Plant;
        //        if (plant == null)
        //            continue;

        //        var allSowablePositions = zone.GetSowingPositions()
        //                                      .Where(g => actor.CanReserve(g) && actor.CanReach(g));
        //        if (!allSowablePositions.Any())
        //            return null;
        //        var destinationsEnumerator = allSowablePositions.GetEnumerator();

        //        var allseeds = actor.Map.GetObjectsLazy().Where(c => c.IsSeedFor(plant) && actor.CanReserve(c)
        //                                                             ).OrderByReachableRegionDistance(actor);
        //        var enumSources = allseeds.GetEnumerator();

        //        var remaining = actor.MaxCarryable(ItemDefOf.Seeds);
        //        var enumTargets = allSowablePositions.GetEnumerator();
        //        var task = new AITask(typeof(TaskBehaviorDeliverMaterials));
        //        while (enumSources.MoveNext() && remaining > 0)
        //        {
        //            var count = 0;
        //            var currentSource = enumSources.Current;
        //            var carryFromCurrent = Math.Min(remaining, currentSource.StackSize);
        //            while (enumTargets.MoveNext() && carryFromCurrent > 0)
        //            {
        //                var currentTarget = enumTargets.Current;
        //                carryFromCurrent--;
        //                remaining--;
        //                count++;
        //                task.AddTarget(TaskBehaviorDeliverMaterials.DestinationID, (map, currentTarget), 1);
        //            }
        //            task.AddTarget(TaskBehaviorDeliverMaterials.MaterialID, currentSource, count);
        //        }
        //        if (task.TargetsA.Any())
        //            return task;
        //    }
        //    return null;
        //}
    }
}
