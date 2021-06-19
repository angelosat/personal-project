using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.Components;
using Start_a_Town_.Components.Needs;
using Start_a_Town_.Net;

namespace Start_a_Town_.AI.Behaviors.Observe
{
    class TaskGiverObserve : TaskGiver
    {
        protected override AITask TryAssignTask(Actor actor)
        {
            var need = actor.GetNeed(NeedDef.Curiosity);


            if (need.Value > 50) //98)// 
                return null;
            //var potentialTargets = actor.Map.GetObjects().Where(o => actor.CanReserve(o));
            var potentialTargets = actor.Map.GetObjects()
                .Where(o=>actor.CanReserve(o));
                //.OrderByReachableRegionDistance(actor);
            //var randomized = new Queue<GameObject>(potentialTargets.Randomize<GameObject>((actor.Net as Server).GetRandom()));
            var randomized = new Queue<GameObject>(potentialTargets.Randomize<GameObject>(actor.Map.Random));

            while (randomized.Count > 0)
            {
                var obj = randomized.Dequeue();
                if (obj.IDType == GameObject.Types.Actor)
                    continue;
                if (obj == actor)
                    continue;
                return new AITask(typeof(BehaviorTaskObserveNew)) { TargetA = new TargetArgs(obj) };
            }
            return null;
        }
    }
}
