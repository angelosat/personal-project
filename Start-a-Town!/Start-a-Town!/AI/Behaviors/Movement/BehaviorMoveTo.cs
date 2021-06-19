using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.Components.Interactions;

namespace Start_a_Town_.AI.Behaviors
{
    class BehaviorMoveTo : BehaviorQueue// BehaviorSequence
    {
        public BehaviorMoveTo(string targetKey, int range)
        {
            this.Children = new List<Behavior>()
            {
                //new BehaviorGetAt(targetKey, range),
                //new BehaviorInteractionNew(targetKey, interaction)
                new BehaviorDomain(new IsAt(targetKey, range),
                    new BehaviorStopMoving()),
                new BehaviorGetAtNewNew(targetKey)//, range)
            };
        }

        public BehaviorMoveTo(TargetArgs targetArgs, int range)
        {
            this.Children = new List<Behavior>()
            {
                new BehaviorDomain(new IsAt(targetArgs, range),
                    new BehaviorStopMoving()),
                new BehaviorGetAtNewNew(targetArgs)//, range)
            };
        }
        public override BehaviorState Execute(Actor parent, AIState state)
        {
            var result = base.Execute(parent, state);
            return result;
        }
        //public override object Clone()
        //{
            
        //    throw new Exception();
        //}
    }
}
