using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.AI;
using Start_a_Town_.AI.Behaviors;
using Start_a_Town_.Components;
using Start_a_Town_.Towns.Farming;

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

            //if (!actor.HasLabor(AILabor.Harvester))
            //    return null;
            //var manager = actor.Map.Town.FarmingManager;
            //foreach (var zone in manager.GrowZones.Values)
            //{
            //    var plants = zone.GetHarvestablePlants()
            //                 .Where(o => actor.CanReserve(o))
            //                 .OrderByReachableRegionDistance(actor);
            //    var plant = plants.FirstOrDefault();
            //    if (plant == null)
            //        continue;
            //    var task = new AITask(TaskDefOf.Harvesting, new TargetArgs(plant));
            //    return task;
            //}
            //return null;
        }
        
    }
}
