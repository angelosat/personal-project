using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Start_a_Town_.Components.AI
{
    class AIIdle : Behavior
    {
        TimeSpan Time { get { return (TimeSpan)this["Time"]; } set { this["Time"] = value; } }
        public override object Clone()
        {
            return new AIIdle();
        }

        int Last = 0;

        public override string Name
        {
            get
            {
                return "Idle";
            }
        }

        public AIIdle() : this(new TimeSpan(0, 0, 1)) { }
        public AIIdle(TimeSpan time)
        {
            this.Time = time;
            Children = new List<Behavior>() { new AIWait(), new AIWander() };
        }

        public override BehaviorState Execute(GameObject parent, AIState state)
        {
            var net = parent.Net;
            Personality personality = state.Personality;
            Knowledge knowledge = state.Knowledge;

            for (int i = Last; i < Children.Count; i++)
            {
                Behavior behav = Children[i];
                if (behav.Execute(parent, state) == BehaviorState.Running)
                {
                    Last = i;
                    return BehaviorState.Running;
                }
            }
            Last = 0;
            return BehaviorState.Success;
        }
    }
}
