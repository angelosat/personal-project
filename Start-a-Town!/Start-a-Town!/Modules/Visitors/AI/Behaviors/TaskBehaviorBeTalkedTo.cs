using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.AI;
using Start_a_Town_.AI.Behaviors;

namespace Start_a_Town_
{
    class TaskBehaviorBeTalkedTo : BehaviorPerformTask
    {
        protected override IEnumerable<Behavior> GetSteps()
        {
            yield return new BehaviorStopMoving();
            var actor = this.Actor;
            var state = actor.GetState();
            var task = this.Task;
            yield return new BehaviorWait(() =>
            {
                //var convo = this.Actor.GetState().ConversationNew;
                //return convo?.Finished ?? true;
                return state.ConversationPartner == null;
            })
            {
                TickAction = () =>
                {
                    actor.FaceTowards(task.TargetA);
                }
            };

            //yield return new BehaviorWait(()=>!this.Actor.Net.Map.World.Population.GetVisitorProperties(this.Actor).RequiresGuidance);
        }
    }
}
