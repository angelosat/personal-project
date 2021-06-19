using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Start_a_Town_.AI;
using Start_a_Town_.Components;
using Start_a_Town_.Components.AI;

namespace Start_a_Town_.AI.Behaviors
{
    class AICoop : Behavior
    {
        GameObject Leader;// { get { return (GameObject)this["Leader"]; } set { this["Leader"] = value; } }
        //Stack<InteractionOld> PlanInteractions;// { get { return (Stack<InteractionOld>)this["PlanInteractions"]; } set { this["PlanInteractions"] = value; } }
        bool Following;// { get { return (bool)this["Following"]; } set { this["Following"] = value; } }
        float ReactionDelay;// { get { return (float)this["ReactionDelay"]; } set { this["ReactionDelay"] = value; } }

        public override string Name
        {
            get
            {
                return "Following: " + Leader;
            }
        }

        public AICoop()
        {
            this.ReactionDelay = Engine.TicksPerSecond / 3f;
            this.Leader = null;
            this.Following = true;
        }

        //public override Behavior Initialize(GameObject parent)
        //{
        //    parent.HandleMessage(Message.Types.Drop);
        //    Leader.HandleMessage(Message.Types.Followed, parent);
        //    return this;
        //}

        //public override Behavior Finalize(GameObject parent)
        //{
        //    Leader.HandleMessage(Message.Types.Unfollowed, parent);
        //    return new AIIdle();
        //}

        public override BehaviorState Execute(Actor parent, AIState state)//IObjectProvider net, GameObject parent, Personality personality, Knowledge knowledge, params object[] p)
        {
            //GameObject parent = state.Parent;
            //IObjectProvider net = state.Net;
            PersonalityComponent personality = parent.Personality;
            Knowledge knowledge = state.Knowledge;
            if (Leader == null)
                return BehaviorState.Fail;

            //InteractionTask task = parent["Control"]["Task"] as InteractionTask;
            //if (task != null)
            //{
            //    //if (task.Interaction.Range >= 0)
            //    //{
            //        //Vector3 difference = (task.Interaction.Source.Global - parent.Global);
            //        //float length = difference.Length();
            //        //if (length > task.Interaction.Range)
            //    if (!task.Interaction.Range( parent, task.Interaction.Source))//length))
            //        {
            //            Vector3 difference = (task.Interaction.Source.Global - parent.Global);
            //            float length = difference.Length();
            //            difference.Normalize();
            //            difference.Z = 0;
            //            throw new NotImplementedException();
            //            //parent.PostMessage(Message.Types.MoveToObject, parent, task.Interaction.Source, task.Interaction.Range, 1f);
            //            return BehaviorState.Running;
            //        }
            //    //}
            //        parent.PostMessage(Message.Types.Perform);
            //    return BehaviorState.Running;
            //}

            if (!Following)
                return BehaviorState.Running;

            Vector3 diff = (Leader.Global - parent.Global);
            float distance = diff.Length();
            if (distance <= 1)
            {
                Console.WriteLine(distance);
                throw new NotImplementedException();
                //GameObject.PostMessage(parent, Message.Types.StopWalking);
                this.ReactionDelay = Engine.TicksPerSecond / 3f;

            }
            else// if (distance > 2)
            {
                if (this.ReactionDelay > 0)
                {
                    ReactionDelay -= 1;// GlobalVars.DeltaTime;
                    return BehaviorState.Running;
                }
                diff.Normalize();
                diff.Z = 0;
                throw new NotImplementedException();
                //parent.PostMessage(Message.Types.Move, parent, diff, 1f);
            }
            //return BehaviorState.Running;
        }

        //public override bool HandleMessage(GameObject parent, ObjectEventArgs e)
        //{
        //    switch (e.Type)
        //    {
        //        //case Message.Types.Attack:
        //        //    if (parent == e.Sender)
        //        //    {
        //        //        parent.HandleMessage(Message.Types.Interrupt, parent);
        //        //        if (!Following)
        //        //        {
        //        //            Following = true;
        //        //            return true;
        //        //        }
        //        //    }
        //        //    return true;
        //        case Message.Types.Activate:
        //            if (parent == e.Sender)
        //            {
        //                throw new NotImplementedException();
        //                //parent.PostMessage(Message.Types.Interrupt, parent);
        //                if (!Following)
        //                {
        //                    Following = true;
        //                    return true;
        //                }
        //            }
        //            if (Leader == e.Sender)
        //            {
        //                throw new NotImplementedException();
        //                //parent.PostMessage(Message.Types.Interrupt, parent);
        //                //Leader.PostMessage(Message.Types.Unfollowed, parent);
        //                Leader = null;

        //                return true;
        //            }
        //            Leader = e.Sender;
        //            if (Leader != null)
        //            {
        //                throw new NotImplementedException();
        //                //Leader.PostMessage(Message.Types.Followed, parent);
        //                Following = true;
        //            }

        //          //  parent.PostMessage(Message.Types.Drop);
        //            return true;

        //        case Message.Types.Order:
        //            Following = false;
        //            InteractionOld inter = e.Parameters[0] as InteractionOld;
        //            throw new NotImplementedException();
        //            //parent.PostMessage(Message.Types.BeginInteraction, parent, inter, e.Parameters[1]);// e.Parameters.Skip(1).ToArray());
        //            return true;

        //        //case Message.Types.BeginInteraction:
        //        //    Following = false;
        //        //    return true;

        //        case Message.Types.Death:
        //            if (Leader == null)
        //                return true;
        //            throw new NotImplementedException();
        //            //Leader.PostMessage(Message.Types.Unfollowed, parent);
        //            Leader = null;

        //            return true;

        //        //case Message.Types.Query:
                    
        //        //    return true;
        //        default:
        //            return false;
        //    }
        //}
        //public override void Query(GameObject parent, List<InteractionOld> list)//GameObjectEventArgs e)
        //{
        //    //List<Interaction> list = e.Parameters[0] as List<Interaction>;
        //   // list.Add(new Interaction(TimeSpan.Zero, Message.Types.Activate, parent, e.Sender == Leader ? "\"Stop following\"" : "\"Follow me\"", range: r => true));// -1));
        //    list.Add(new InteractionOld(TimeSpan.Zero, Message.Types.ManageEquipment, parent, "Manage Equipment", range: (r1, r2) => true));//-1));
        //}
        //public override Conversation.States HandleConversation(GameObject parent, GameObject speaker, string option, out string speech, out DialogueOptionCollection options)
        //{
        //    return base.HandleConversation(parent, speaker, option, out speech, out options);
        //}

        //public override void GetDialogueOptions(GameObject parent, GameObject speaker, DialogueOptionCollection options)
        //{
        //    options.AddRange(new DialogueOption[]{
        //    "Follow me".ToDialogueOption(HandleConversation, ()=> speaker != Leader),
        //    "Stop following".ToDialogueOption(HandleConversation, ()=> speaker == Leader),
        //    });
        //}

        //public override Conversation.States HandleConversation(GameObject parent, GameObject speaker, string option, out string speech, out DialogueOptionCollection options)
        //{
        //    speech = "";
        //    switch (option)
        //    {
        //        case "Follow me":
        //            speech = "Lead the way.";
        //            this.Leader = speaker;
        //            options = new DialogueOptionCollection();
        //            return Conversation.States.Finished;
        //        case "Stop following":
        //            speech = "I'll be around if you need me.";
        //            this.Leader = null;
        //            break;
        //        default:
        //            break;
        //    }
        //    speech += "\n\nAnything else I can help you with?";
        //    options = parent.GetDialogueOptions(speaker);
        //    return Conversation.States.InProgress;
        //}
        public override object Clone()
        {
            return new AICoop();
        }
    }
}
