using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_
{
    class InteractionConversationGradual : InteractionPerpetual
    {
        int CurrentTickInt;
        ConversationTopic Topic;
        public InteractionConversationGradual(ConversationTopic topic) : base("Chatting")
        {
            this.Topic = topic;
            this.Verb = this.Name;
            //this.TickCount = topic.MaxTicks;
        }
        public InteractionConversationGradual() : base("Chatting")
        {
            this.Verb = this.Name;
        }
        public override object Clone()
        {
            return new InteractionConversation(this.Topic);
        }

        public override void OnUpdate(GameObject a, TargetArgs t)
        {
            var actor = a as Actor;
            this.Topic.ApplyNew(actor, t.Object as Actor);
            this.CurrentTickInt++;
            if (this.CurrentTickInt >= this.Topic.MaxTicks)
            {
                actor.FinishConversation();
                this.Finish(a, t);
            }
        }

        //public override void Perform(GameObject actor, TargetArgs target)
        //{
        //    var a = actor as Actor;
        //    //if (actor.Net is Net.Client)
        //        //return;
        //    a.TalkTo(target.Object as Actor, this.Topic);// ?? a.GetNextConversationTopicFor(target.Object as Actor));
        //    a.FinishConversation();
        //}
        protected override void WriteExtra(BinaryWriter w)
        {
            
            this.Topic.Write(w);
            //w.Write(this.TickCount);
        }
        protected override void ReadExtra(BinaryReader r)
        {
          
            this.Topic = Def.GetDef<ConversationTopic>(r.ReadString());
            //this.TickCount = this.Topic.MaxTicks;//= r.ReadInt32();
        }

       
    }
}
