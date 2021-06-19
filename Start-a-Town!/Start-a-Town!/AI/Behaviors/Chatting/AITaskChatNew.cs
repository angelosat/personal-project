using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_.AI.Behaviors.Chatting
{
    class AITaskChatNew : AITask
    {
        public AIConversationManager.Conversation Convo;
        public override string Name
        {
            get
            {
                return "Chatting";
            }
        }
        public AITaskChatNew(AIConversationManager.Conversation convo)
        {
            this.Convo = convo;
            //this.Behavior = new TaskBehaviorChat(this);
        }
        public AITaskChatNew()
        {

        }
        //AIConversationManager.Conversation Conversation;
        static readonly BehaviorChattingNew Behav = new BehaviorChattingNew();
        public override Behavior GetBehavior(GameObject actor)
        {
            return new BehaviorChattingNew(); // Behav;
        }
    }
}
