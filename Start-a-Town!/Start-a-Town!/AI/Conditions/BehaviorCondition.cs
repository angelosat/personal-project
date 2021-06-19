using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_.AI.Behaviors
{
    //abstract class BehaviorCondition
    //{
    //    public abstract bool Evaluate(GameObject agent, AIState state);
    //}
    public class BehaviorCondition : Behavior
    {
        readonly Func<GameObject, AIState, bool> Condition;
        public BehaviorCondition()
        {

        }
        public BehaviorCondition(Func<GameObject, AIState, bool> condition)
        {
            this.Condition = condition;
        }
        public virtual bool Evaluate(GameObject agent, AIState state)
        {
            return this.Condition(agent, state);
        }
        public override BehaviorState Execute(Actor parent, AIState state)
        {
            //var result = this.Condition(parent, state);
            var result = this.Evaluate(parent, state);
            return result ? BehaviorState.Success : BehaviorState.Fail;
        }
        public override object Clone()
        {
            throw new NotImplementedException();
            //return new BehaviorCondition(this.Condition);
        }

        new public ConditionNot Not()
        {
            return new ConditionNot(this);
        }
    }
}
