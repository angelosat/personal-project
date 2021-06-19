using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Start_a_Town_.Components.AI
{
    class AIWait : Behavior
    {
        public override string Name
        {
            get
            {
                return "Idle";
            }
        }
        TimeSpan Timer { get { return (TimeSpan)this["Timer"]; } set { this["Timer"] = value; } }
        TimeSpan T { get { return (TimeSpan)this["T"]; } set { this["T"] = value; } }

        //public AIWait() : this(new TimeSpan(0, 0, 5)) { }
        public AIWait() : this(new TimeSpan(0, 0, 1)) { }

        public AIWait(TimeSpan timer)
        {
            this.Timer = timer;
            this.T = timer;
        }

        // TODO: make Timer based on how nervous the personality is

        public override BehaviorState Execute(GameObject parent, AIState state)
        {
            TimeSpan sub = new TimeSpan(0, 0, 0, 0, (int)(100 / 6f));
            T = T.Subtract(sub);

            if (T.TotalMilliseconds > 0)
                return BehaviorState.Running;
            T = TimeSpan.FromSeconds(10 * state.Personality.Composure.Normalized);
            return BehaviorState.Success;
        }

        public override object Clone()
        {
            return new AIWait();
        }
    }
}
