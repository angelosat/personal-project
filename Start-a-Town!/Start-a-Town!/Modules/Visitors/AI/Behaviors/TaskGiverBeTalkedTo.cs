using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_
{
    class TaskGiverBeTalkedTo : TaskGiver
    {
        protected override AITask TryAssignTask(Actor actor)
        {
            var state = actor.GetState();
            //if (state.ConversationNew == null)
            if(state.ConversationPartner == null)
                return null;
            return new AITask(typeof(TaskBehaviorBeTalkedTo));
        }
        //protected override AITask TryAssignTask(Actor actor)
        //{
        //    //TODO check if there's pending guidance
        //    var props = actor.Map.World.Population.GetVisitorProperties(actor);
        //    if (props.Guide == null)
        //        return null;
        //    return new AITask(typeof(TaskBehaviorReceiveGuidance));
        //}
    }
}
