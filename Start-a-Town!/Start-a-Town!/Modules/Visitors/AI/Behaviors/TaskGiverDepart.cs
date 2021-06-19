using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.GameModes.StaticMaps;

namespace Start_a_Town_
{
    class TaskGiverDepart : TaskGiver
    {
        const int MaxTries = 5;
        protected override AITask TryAssignTask(Actor actor)
        {
            var visitor = actor.GetVisitorProperties();
            var chance = visitor.GetDepartChance();
            if (actor.Map.World.Random.Chance(chance))
            {
                var map = actor.Map as StaticMap;
                for (int i = 0; i < MaxTries; i++)
                {
                    var exit = map.GetRandomEdgeCell().Above();
                    if (actor.CanReachNew(exit))
                        return new AITask(typeof(TaskBehaviorDepart), (map, exit));
                }
                actor.Net.Report($"Failed to find a reachable exit for {actor.Name}'s departure");
            }
            //actor.Net.Report($"Failed to depart with chance {chance}");
            return null;
        }
    }
}
