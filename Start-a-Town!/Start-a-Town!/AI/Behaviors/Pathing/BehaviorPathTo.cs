using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_.AI.Behaviors
{
    class BehaviorPathTo : BehaviorQueue
    {
        string TargetKey;
        public BehaviorPathTo(string targetKey)
        {
            this.TargetKey = targetKey;
            this.Children = new List<Behavior>()
            {
                new BehaviorSequence(
                    new BehaviorChaseTarget("step", .1f),
                    new BehaviorNextPathStep("path", "step")),
                new BehaviorSequence(
                    new BehaviorFindPathNew(targetKey, "path"),
                    new BehaviorNextPathStep("path", "step"))
            };
        }
        public override BehaviorState Execute(Actor parent, AIState state)
        {
            return base.Execute(parent, state);
        }
        //public override BehaviorState Execute(Entity parent, AIState state)
        //{
        //    throw new NotImplementedException();
        //}
        //public override object Clone()
        //{
        //    throw new NotImplementedException();
        //}
    }
}
