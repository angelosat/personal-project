using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.Components.Needs;
using Start_a_Town_.Components;

namespace Start_a_Town_.AI
{
    public class AIConversationManager
    {
        public List<Conversation> Conversations = new List<Conversation>();
        public Conversation Start(GameObject initiator, GameObject target)
        {
            var c = new Conversation(initiator, target);
            this.Conversations.Add(c);
            return c;
        }
        public class Conversation
        {
            public enum States { Requested, Accepted, Declined, Started, Finished}
            public States State;
            public GameObject Initiator, Target;
            public Progress Progress = new Progress(0, Engine.TicksPerSecond, 0);
            public Conversation(GameObject initiator, GameObject target)
            {
                this.Initiator = initiator;
                this.Target = target;
            }

            internal void Tick()
            {
                switch (this.State)
                {
                    case States.Accepted:
                        this.State = States.Started;
                        break;

                    case States.Started:
                        var prog = this.Progress;
                        prog.Value += 1;
                        if (prog.Percentage >= 1)
                        {
                            prog.Value = 0;
                            //var socialInitiator = NeedsComponent.GetNeed(this.Initiator, Need.Types.Social);
                            var socialInitiator = this.Initiator.GetNeed(NeedDef.Social);
                            //socialInitiator.SetValue(socialInitiator.Value + 1, this.Initiator);
                            //PacketNeedModify.Send(this.Initiator.Net as Server, this.Initiator.InstanceID, (int)Need.Types.Social, 1);
                            NeedsComponent.ModifyNeed(this.Initiator, NeedDef.Social, 1);
                            if (socialInitiator.Percentage == 1)
                                this.State = AIConversationManager.Conversation.States.Finished;

                            //var socialTarget = NeedsComponent.GetNeed(this.Target, Need.Types.Social);
                            var socialTarget = this.Target.GetNeed(NeedDef.Social);

                            //socialTarget.SetValue(socialTarget.Value + 1, this.Target);
                            //PacketNeedModify.Send(this.Target.Net as Server, this.Target.InstanceID, (int)Need.Types.Social, 1);
                            NeedsComponent.ModifyNeed(this.Target, NeedDef.Social, 1);
                        }
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
