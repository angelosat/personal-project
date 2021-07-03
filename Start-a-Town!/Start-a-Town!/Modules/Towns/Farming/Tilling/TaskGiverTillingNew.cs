using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.AI;
using Start_a_Town_.AI.Behaviors;
using Start_a_Town_.Components;
using Start_a_Town_.Towns.Farming;
using Microsoft.Xna.Framework;

namespace Start_a_Town_
{
    class TaskGiverTillingNew : TaskGiver
    {
        protected override AITask TryAssignTask(Actor actor)
        {
            if (!actor.HasLabor(JobDefOf.Farmer))
                return null;
            //var manager = actor.Map.Town.FarmingManager;
            //var allTasks = manager.GetAllTillingLocations()
            //    .Where(g=>actor.CanReserve(g))
            //    .OrderByReachableRegionDistance(actor);
            var map = actor.Map;
            var allTasks = GetAllTillingLocations(map)
                .Where(g => actor.CanReserve(g))
                .OrderByReachableRegionDistance(actor);
            var equipped = actor.GetEquipmentSlot(GearType.Mainhand);

            if (!allTasks.Any())
                return TaskHelper.TryStoreEquipped(actor, GearType.Mainhand);

            var closest = allTasks.OrderBy(t => Vector3.DistanceSquared(actor.Global, t)).First();
            var targets = new Queue<TargetArgs>(allTasks.Select(t => new TargetArgs(map, t)));
            var task = new AITask(typeof(TaskBehaviorTillingNew));// { TargetA = new TargetArgs(actor.Map, closest) };
            task.SetTarget(TaskBehaviorTillingNew.TargetInd, new TargetArgs(map, closest));

            FindTool(actor, task, ToolAbilityDef.Argiculture);//, TaskBehaviorTillingNew.ToolInd);
            return task;
        }

        static IEnumerable<IntVec3> GetAllTillingLocations(IMap map)
        {
            foreach (var zone in map.Town.ZoneManager.GetZones<GrowingZone>())
                foreach (var pos in zone.GetTillingPositions())
                    yield return pos;
        }
    }
}
