using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_
{
    class TaskGiverOfferGuidance : TaskGiver
    {
        protected override AITask TryAssignTask(Actor actor)
        {
            if (!actor.HasLabor(JobDefOf.Guide))
                return null;
            //var visitors = actor.Map.World.Population.Find(v => v.Actor.Map == actor.Map && v.RequiresGuidance);
            var visitors = actor.Map.World.Population.Find(v => v.Actor.Map == actor.Map && v.Actor.GetNeed(VisitorNeedsDefOf.Guidance).Value < 50);

            // TODO sort visitors here by urgency
            var visitor = visitors.FirstOrDefault();
            if (visitor == null)
                return null;
            //if (visitor.GetTimeElapsed(actor.Net.Clock).TotalSeconds < 2)
            var elapsed = visitor.GetTimeElapsed();
            if (elapsed.TotalSeconds < 2)
                return null;
            if (visitor.Actor.CurrentTask != null)
                return null;
            //if (visitor.Actor.GetState().ConversationNew != null)
            //    return null;
            if (visitor.Actor.GetState().ConversationPartner != null)
                return null;
            //visitor.Guide = actor; //TODO do this inside the behavior instead of here?
            //var convo = new ConversationNew(actor, visitor.Actor);
            ////visitor.Actor.GetState().ConversationNew = convo;
            //convo.Start();
            visitor.Actor.GetState().ConversationPartner = actor;
            actor.GetState().ConversationPartner = visitor.Actor;
            actor.EnqueueCommunication(visitor.Actor, ConversationTopic.Guidance);
            return new AITask(TaskDefOf.Chatting, new TargetArgs(visitor.Actor));
        }
    }
}
