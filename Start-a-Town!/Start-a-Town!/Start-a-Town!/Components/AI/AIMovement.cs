using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components.Interactions;

namespace Start_a_Town_.Components.AI
{
    class AIMovement : Behavior
    {
        public override string Name
        {
            get
            {
                return "AIMovement";
            }
        }

        public override BehaviorState Execute(GameObject parent, AIState state)
        {
            AIPlanStep step = state.Plan.Peek();
            Script goal = step.Script;
            TargetArgs target = step.Target;
            if (!target.Object.Exists)
            {
                //parent.Control.FinishScript(Script.Types.Walk, new ScriptArgs(net, parent, target));
                parent.TryGetComponent<MobileComponent>(c => c.Stop(parent));
                return BehaviorState.Fail;
            }
           // throw new NotImplementedException();
            if (goal.RangeCheck(parent, target, goal.RangeValue))
            {
                //parent.Control.FinishScript(Script.Types.Walk, new ScriptArgs(net, parent, target));
                parent.TryGetComponent<MobileComponent>(c => c.Stop(parent));
                return BehaviorState.Success;
            }
            float range = goal.RangeValue;
            Vector3 difference = (target.FinalGlobal - parent.Global);
            Vector3 direction;
            Vector3.Normalize(ref difference, out direction);
            parent.Direction = direction;
            //parent.Control.TryStartScript(Script.Types.Walk, new ScriptArgs(net, parent, target));
            parent.TryGetComponent<MobileComponent>(c => c.Start(parent));
            return BehaviorState.Running;
        }

        public override object Clone()
        {
            return new AIMovement();
        }
    }
}
