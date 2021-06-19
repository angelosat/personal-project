using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.AI;
using Start_a_Town_.Components.AI;
using Start_a_Town_.Components;

namespace Start_a_Town_
{
    public class AnyCheck : ScriptTaskCondition
    {
        public override void GetChildren(List<ScriptTaskCondition> list)
        {
            foreach (var item in this.Children)
                item.GetChildren(list);
        }
        public AnyCheck(params ScriptTaskCondition[] conditions)
            : base("Any")
        {
            this.Children = new List<ScriptTaskCondition>(conditions);
            this.ErrorEvent = Message.Types.InvalidTarget;
        }
        public override bool Condition(GameObject a, TargetArgs t)
        {
            foreach (var item in this.Children)
                if (item.Condition(a, t))
                    return true;
            return false;
        }
        public override ScriptTaskCondition GetFailedCondition(GameObject a, TargetArgs t)
        {
            foreach (var item in this.Children)
            {
                var innerItem = item.GetFailedCondition(a, t);
                if (innerItem == null)
                    return null;
            }
            return this;
        }

        public override void AIInit(GameObject agent, TargetArgs target, AIState state)
        {
            foreach (var item in this.Children)
                item.AIInit(agent, target, state);
        }

        public override bool AITrySolve(GameObject agent, TargetArgs target, AIState state, List<AIInstruction> solution)
        {
            foreach (var cond in this.Children)
            {
                if (cond.Condition(agent, target))
                    return true;
                else
                    if (cond.AITrySolve(agent, target, state, solution))
                        return true;
            }
            return false;
        }
    }
}
