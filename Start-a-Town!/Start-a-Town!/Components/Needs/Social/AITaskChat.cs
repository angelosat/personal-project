using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.AI;

namespace Start_a_Town_.Components.Needs.Social
{
    class AITaskChat : AITask
    {
        static readonly AIBehaviorChat Behav = new AIBehaviorChat();
        public override Behavior GetBehavior(GameObject actor)
        {
            return Behav;
        }
    }
}
