using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_.AI.Behaviors
{
    class BehaviorStoreVariable : Behavior
    {
        string VarName;
        Func<GameObject, AIState, object> ValueExtractor;
        public BehaviorStoreVariable(string name, Func<GameObject, AIState, object> valueExtractor)
        {
            this.VarName = name;
            this.ValueExtractor = valueExtractor;
        }
        public override BehaviorState Execute(Actor parent, AIState state)
        {
            state.Blackboard[this.VarName] = this.ValueExtractor(parent, state);
            return BehaviorState.Success;
        }
        public override object Clone()
        {
            return new BehaviorStoreVariable(this.VarName, this.ValueExtractor);
        }
    }
}
