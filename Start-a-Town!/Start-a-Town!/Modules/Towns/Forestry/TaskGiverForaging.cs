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
    class TaskGiverForaging : TaskGiver
    {
        protected override AITask TryAssignTask(Actor actor)
        {
            if (!actor.HasLabor(JobDefOf.Forager))
                return null;
            var manager = actor.Map.Town.ChoppingManager;
            var plants = manager.GetPlants()
                         .Where(o => actor.CanReserve(o))
                         .OrderByReachableRegionDistance(actor);
            var plant = plants.FirstOrDefault();
            if (plant == null)
                return null;
            var task = new AITask(typeof(TaskBehaviorHarvestingNew)) { TargetA = new TargetArgs(plant) };
            return task;
        }
    }
}
