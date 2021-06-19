using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Start_a_Town_.Components.AI
{
    class AIMemory : Behavior
    {
        float Timer { get; set; }
        float Period { get; set; }
        public AIMemory()
        {
            this.Timer = 0;
            this.Period = Engine.TargetFps;
        }
        public override BehaviorState Execute(GameObject parent, AIState state)
        {
            if (this.Timer < Period)
            {
                this.Timer++;
                return BehaviorState.Running;
            }
            this.Timer = 0;
            state.Knowledge.Update();
            return BehaviorState.Running;
        }

        public override object Clone()
        {
            return new AIMemory();
        }
    }
}
