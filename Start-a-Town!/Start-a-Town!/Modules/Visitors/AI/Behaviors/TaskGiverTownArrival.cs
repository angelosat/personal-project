using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.GameModes.StaticMaps;

namespace Start_a_Town_
{
    class TaskGiverTownArrival : TaskGiver
    {
        protected override AITask TryAssignTask(Actor actor)
        {
            var props = actor.Map.World.Population.GetVisitorProperties(actor);
            if (!props.HangAroundSpot.HasValue)
                props.HangAroundSpot = this.FindHangSpot(actor);
            var spot = props.HangAroundSpot.Value;
            var distance = Vector3.Distance(actor.Global, spot);
            if (distance < 10)
                return null;

            var task = new AITask(TaskDefOf.Moving, new TargetArgs(spot)) { Urgent = false };

            return task;
        }

        Vector3 FindHangSpot(Actor actor)
        {
            var town = actor.Town;
            var citizens = town.GetAgents().Randomize(town.Map.Random);
            foreach(var citizen in citizens)
            {
                //return citizen.Global.SnapToBlock();
                foreach (var spot in citizen.Global.SnapToBlock().GetRadial(3))
                {
                    if(actor.Map.Contains(spot) && actor.CanStandIn(spot) && actor.CanReach(spot))
                    {
                        return spot;
                    }
                }
            }
            return actor.Global;
        }
    }
}
