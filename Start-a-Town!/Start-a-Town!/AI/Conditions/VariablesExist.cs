using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_.AI.Behaviors
{
    class VariablesExist : BehaviorCondition
    {
        string[] VariableName;
        public VariablesExist(params string[] name)
        {
            this.VariableName = name;
        }
        public override bool Evaluate(GameObject agent, AIState state)
        {
            //return state.Blackboard.ContainsKey(this.VariableName);
            foreach (var name in this.VariableName)
                if (!state.Blackboard.ContainsKey(name))
                    return false;
            return true;
        }
    }
}
