using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.Components;

namespace Start_a_Town_.AI.Behaviors
{
    class IsHauling : BehaviorCondition
    {
        Func<GameObject, bool> Condition;
        public IsHauling(Func<GameObject, bool> condition)
        {
            this.Condition = condition;
        }
        public override bool Evaluate(GameObject agent, AIState state)
        {
            var hauled = PersonalInventoryComponent.GetHauling(agent).Object;
            return this.Condition(hauled);
        }
    }
}
