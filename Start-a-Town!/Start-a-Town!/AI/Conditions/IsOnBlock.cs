using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.AI.Behaviors
{
    class IsOnBlock : BehaviorCondition
    {
        TargetArgs Target;
        string VariableName;
        public IsOnBlock(string variableName)
        {
            this.VariableName = variableName;
        }
        public IsOnBlock(TargetArgs target)
        {
            this.Target = target;
        }

        public override bool Evaluate(GameObject agent, AIState state)
        {
            var target = this.Target ?? state.Blackboard[this.VariableName] as TargetArgs;
            var standingOn = agent.Map.GetCell(agent.Global + new Vector3(0, 0, Components.PhysicsComponent.Gravity));
            var targetCell = agent.Map.GetCell(target.Global);
            return standingOn == targetCell;
        }
    }
}
