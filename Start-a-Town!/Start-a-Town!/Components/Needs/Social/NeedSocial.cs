using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.Components.Interactions;
using Start_a_Town_.Components.AI;
using Start_a_Town_.AI;
using Start_a_Town_.AI.Behaviors.Chatting;
using Start_a_Town_.Components.Needs.Social;
using Start_a_Town_.Components.Needs;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.Components
{
    class NeedSocial : Need
    {
        public NeedSocial()
            : base(Need.Types.Social, "Social", 100)//, .1f)//, 10)
        {

        }
        public override float Threshold
        {
            get
            {
                return 98;
            }
        }
        static readonly BehaviorInitiateConversation Behav = new BehaviorInitiateConversation();// new AIBehaviorChat();
        static readonly TaskGiverChat _TaskGiver = new TaskGiverChat();
        public override TaskGiver TaskGiver
        {
            get
            {
                return _TaskGiver;
            }
        }
        
        public override AITask GetTask(GameObject parent)
        {
            var threshold = 50;
            if (this.Value >= threshold)
                return null;
            //return new AITaskChat();
            return new AITaskChatNew();
        }
        
        public override object Clone()
        {
            return new NeedSocial();
        }
    }
}
