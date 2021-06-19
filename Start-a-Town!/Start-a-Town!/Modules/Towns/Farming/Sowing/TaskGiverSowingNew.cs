using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.AI;
using Start_a_Town_.AI.Behaviors;
using Start_a_Town_.Towns.Farming;
using Microsoft.Xna.Framework;

namespace Start_a_Town_
{
    class TaskGiverSowingNew : TaskGiver
    {
        protected override AITask TryAssignTask(Actor actor)
        {
            var map = actor.Map;
            if (!actor.HasLabor(JobDefOf.Farmer))
                return null;
            // TODO: iterate through all zones until one with an available seed type is found
            //var zone = manager.GetFirstZoneWithAvailableSowingTasks();
            //if (zone == null)
            //    return null;

            //var manager = actor.Map.Town.FarmingManager;
            //var zones = manager.GrowZones.Values;//.GetZonesWithAvailableSowingTasks();
            var zones = map.Town.ZoneManager.GetZones<GrowingZone>();
            foreach (var zone in zones)
            {
                var plant = zone.Plant;
                if (plant == null)
                    continue;

                var allSowablePositions = zone.GetSowingPositions()
                                              .Where(g => actor.CanReserve(g) && actor.CanReach(g));
                if (!allSowablePositions.Any())
                    return null;
                var destinationsEnumerator = allSowablePositions.GetEnumerator();

                var allseeds = actor.Map.GetObjectsLazy().Where(c => c.IsSeedFor(plant) && actor.CanReserve(c)
                                                                     ).OrderByReachableRegionDistance(actor);
                var enumSources = allseeds.GetEnumerator();
                //var carried = actor.GetHauled();
                //if (carried != null && carried.IsSeedFor(seedType))
                //    throw new Exception();

                var remaining = actor.MaxCarryable(ItemDefOf.Seeds);
                var enumTargets = allSowablePositions.GetEnumerator();
                var task = new AITask(typeof(TaskBehaviorDeliverMaterials));
                while (enumSources.MoveNext() && remaining > 0)
                {
                    var count = 0;
                    var currentSource = enumSources.Current;
                    var carryFromCurrent = Math.Min(remaining, currentSource.StackSize);
                    while (enumTargets.MoveNext() && carryFromCurrent > 0)
                    {
                        var currentTarget = enumTargets.Current;
                        carryFromCurrent--;
                        remaining--;
                        count++;
                        task.AddTarget(TaskBehaviorDeliverMaterials.DestinationID, (map, currentTarget), 1);
                    }
                    task.AddTarget(TaskBehaviorDeliverMaterials.MaterialID, currentSource, count);
                }
                if (task.TargetsA.Any())
                    return task;
            }
            return null;
        }



       
        
    }
}
