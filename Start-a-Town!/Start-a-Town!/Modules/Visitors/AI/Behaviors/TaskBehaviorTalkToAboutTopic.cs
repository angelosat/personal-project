using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.AI;
using Start_a_Town_.AI.Behaviors;

namespace Start_a_Town_
{
    class TaskBehaviorTalkToAboutTopic : BehaviorPerformTask
    {
        protected override IEnumerable<Behavior> GetSteps()
        {
            //yield return new BehaviorCustom()
            //{
            //    InitAction = () =>
            //    {
            //        var actor = this.Actor;
            //        var visitor = this.Task.TargetA.Object as Actor;
            //        var convo = new ConversationNew(actor, visitor);
            //        visitor.GetState().ConversationNew = convo;
            //    }
            //};
            yield return new BehaviorGetAtNewNew(TargetIndex.A);
            //yield return new BehaviorCustom()
            //{
            //    InitAction = () =>
            //    {
            //        //var actor = this.Actor;
            //        var target = this.Task.TargetA.Object as Actor;
            //        var st = target.GetState();
            //        //var convo = st.ConversationNew;
            //        //st.ConversationPartner = this.Actor;
            //        //this.Actor.GetState().ConversationPartner = target;
            //        //convo.Advance(this.Actor, ConversationTopic.Guidance);
            //        var topic = this.Actor.GetNextConversationTopicFor(target);
            //        this.Actor.TalkAbout(topic);
            //        this.Actor.FinishConversation();
            //        //convo.Advance(this.Actor, topic);
            //        //convo.Finish();
            //    }
            //};
            yield return new BehaviorInteractionNew(TargetIndex.A, () => new InteractionConversationGradual(this.Actor.GetNextConversationTopicFor(this.Task.TargetA.Object as Actor)));

            //yield return new BehaviorInteractionNew(TargetIndex.A, () => new InteractionConversation(this.Actor.GetNextConversationTopicFor(this.Task.TargetA.Object as Actor)));
        }
        //protected override IEnumerable<Behavior> GetSteps()
        //{
        //    yield return new BehaviorGetAtNewNew(TargetIndex.A);
        //    yield return new BehaviorCustom()
        //    {
        //        InitAction = () =>
        //        {
        //            var actor = this.Actor;
        //            var visitor = this.Task.TargetA.Object as Actor;
        //            var convo = new ConversationNew(actor, visitor);
        //            visitor.GetState().ConversationNew = convo;
        //            //var props = actor.Map.World.Population.GetVisitorProperties(visitor);
        //            //props.RequiresGuidance = false;
        //            //props.Guide = null;
        //            //actor.Net.Write(string.Format("{0} received guidance by {1}", visitor.Name, actor.Name));
        //        }
        //        //InitAction = () =>
        //        //{
        //        //    var actor = this.Actor;
        //        //    var visitor = this.Task.TargetA.Object as Actor;
        //        //    var props = actor.Map.World.Population.GetVisitorProperties(visitor);
        //        //    props.RequiresGuidance = false;
        //        //    props.Guide = null;
        //        //    actor.Net.Write(string.Format("{0} received guidance by {1}", visitor.Name, actor.Name));
        //        //}
        //    };
        //}
    }
}
