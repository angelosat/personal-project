using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Start_a_Town_.Components.AI
{
    class AICondition : Behavior
    {
        //Predicate<Personality> Condition { get { return (Predicate<Personality>)this["Condition"]; } set { this["Condition"] = value; } }

        //public AICondition(Predicate<Personality> condition)
        //{
        //    this.Condition = condition;
        //}

        public override string ToString()
        {
            return "Evaluating condition";
        }

        Predicate<GameObject> Condition { get { return (Predicate<GameObject>)this["Condition"]; } set { this["Condition"] = value; } }

        public AICondition(Predicate<GameObject> condition)
        {
            this.Condition = condition;
        }

        public override BehaviorState Execute(GameObject parent, Personality personality, Knowledge knowledge, params object[] p)
        {
            if (Condition(parent)) //personality))
                return BehaviorState.Success;
            return BehaviorState.Fail;
        }
    }
}
