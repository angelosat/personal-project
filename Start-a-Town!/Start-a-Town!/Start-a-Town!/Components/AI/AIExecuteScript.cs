using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_.Components.AI
{
    class AIExecuteScript : Behavior
    {
        public override string Name
        {
            get
            {
                return "AIExecuteScript";
            }
        }

        public override BehaviorState Execute(GameObject parent, AIState state)
        {
            var net = parent.Net;
            //switch (state.Goal.ScriptState)
            AIPlanStep step = state.Plan.Peek();
            Script goal = step.Script;
            TargetArgs target = step.Target;
            switch (goal.ScriptState)
            {
                case ScriptState.Unstarted:
                    parent.Control.TryStartScript(goal, new ScriptArgs(net, parent, target));
                    return BehaviorState.Running;

                case ScriptState.Running:
                    return BehaviorState.Running;

                case ScriptState.Finished:
                    state.Target = null;
                    //state.Goal = null;
                    state.Plan.Dequeue();
                    return BehaviorState.Success;
            }
            state.Plan.Clear();
            return BehaviorState.Fail;
        }

        public override object Clone()
        {
            return new AIExecuteScript();
        }
    }
}
