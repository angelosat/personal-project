using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.AI;
using Start_a_Town_.Components;
using Start_a_Town_.Components.AI;

namespace Start_a_Town_.AI.Behaviors
{
    class AIFollow : Behavior
    {
        public override string Name
        {
            get
            {
                return "AIFollow";
            }
        }
        public override object Clone()
        {
            return new AIFollow();
        }

        public override string ToString()
        {
            return "Following: " + (this.Leader != null ? this.Leader.Name : "<none>");
        }

        GameObject Leader;
        AITarget Target;

        public AIFollow()
        {
            this.Children = new List<Behavior>(){
                new AIMoveTo()
            };
        }

        public override BehaviorState Execute(GameObject parent, AIState state)
        {
            if(this.Leader !=null)
            {
                state.MoveTarget = this.Target;
                this.Children.First().Execute(parent, state);
                return BehaviorState.Success;
            }

            return this.Leader != null ? BehaviorState.Success : BehaviorState.Fail;
        }

        public override bool HandleMessage(GameObject parent, ObjectEventArgs e)
        {
            switch(e.Type)
            {
                case Components.Message.Types.Follow:
                    break;

                default:
                    break;
            }
            return false;
        }

        protected override void HandleCommunication(GameObject parent, GameObject sender, string option)
        {
            switch(option)
            {
                case "Follow me":
                    //if (this.Leader != null)
                    //    return;
                    //this.Leader = sender;
                    //parent.Net.EventOccured(Message.Types.Chat, parent, "Lead the way, " + sender.Firstname);
                    this.SetLeader(parent, sender);
                    //(parent.Net as Net.Server).RemoteProcedureCall(new TargetArgs(parent), Message.Types.AISetLeader, Net.Network.Serialize(w => w.Write(sender.InstanceID)), parent.Transform.Global);
                    (parent.Net as Net.Server).RemoteProcedureCall(new TargetArgs(parent), Message.Types.AISetLeader, BitConverter.GetBytes(sender.InstanceID), parent.Transform.Global);
                    (parent.Net as Net.Server).AIHandler.AIConversationFinish(parent, "Lead the way, " + sender.Firstname + ".");
                    break;

                case "Stop following":
                    this.Leader = null;
                    this.Target = null;
                    (parent.Net as Net.Server).RemoteProcedureCall(new TargetArgs(parent), Message.Types.AIStopFollowing, parent.Transform.Global);
                    (parent.Net as Net.Server).AIHandler.AIConversationFinish(parent, "Ok, I'll stop following.");
                    break;

                default:
                    break;
            }
        }

        internal override void HandleRPC(GameObject parent, Message.Types type, System.IO.BinaryReader r)
        {
            switch (type)
            {
                case Message.Types.AISetLeader:
                    var leader = parent.Net.GetNetworkObject(r.ReadInt32());
                    this.SetLeader(parent, leader);
                    break;

                case Message.Types.AIStopFollowing:
                    this.Leader = null;
                    this.Target = null;
                    break;

                default:
                    break;
            }
        }
        void SetLeader(GameObject parent, GameObject leader)
        {
            if (this.Leader != null)
                return;
            this.Leader = leader;
            this.Target = new AITarget(new TargetArgs(this.Leader), 1, 3, 5);
            //parent.Net.EventOccured(Message.Types.Chat, parent, "Lead the way, " + leader.Firstname);
        }
        public override void GetDialogOptions(GameObject parent, GameObject speaker, List<DialogOption> options)
        {
            if(this.Leader == null)
                options.Add(new DialogOption("Follow me", parent));
            else
                options.Add(new DialogOption("Stop following", parent));
        }
    }
}
