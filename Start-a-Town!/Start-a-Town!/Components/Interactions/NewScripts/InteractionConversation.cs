using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_
{
    class InteractionConversation : Interaction//Perpetual
    {
        ConversationTopic Topic;
        public InteractionConversation(ConversationTopic topic) :base("Chatting", 5)
        {
            this.Topic = topic;
            this.Verb = this.Name;
        }
        public InteractionConversation() : base("Chatting", 5)
        {
            this.Verb = this.Name;
        }
        public override object Clone()
        {
            return new InteractionConversation(this.Topic);
        }

        //public override void OnUpdate(GameObject a, TargetArgs t)
        //{
        //    throw new NotImplementedException();
        //}

        //internal override void InitAction(GameObject actor, TargetArgs target)
        public override void Perform(GameObject actor, TargetArgs target)
        {
            var a = actor as Actor;
            //if (actor.Net is Net.Client)
                //return;
            a.TalkTo(target.Object as Actor, this.Topic);// ?? a.GetNextConversationTopicFor(target.Object as Actor));
            a.FinishConversation();
        }
        protected override void WriteExtra(BinaryWriter w)
        {
 
            this.Topic.Write(w);
        }
        protected override void ReadExtra(BinaryReader r)
        {
      
            this.Topic = Def.GetDef<ConversationTopic>(r.ReadString());
        }
    }
}
