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
        }
        public AITaskChatNew()
        {

        }
        public override Behavior GetBehavior(GameObject actor)
        {
            return new BehaviorChattingNew();
        }
    }
}
