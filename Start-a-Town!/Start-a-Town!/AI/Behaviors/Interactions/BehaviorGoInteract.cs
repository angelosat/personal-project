using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.Components.Interactions;

namespace Start_a_Town_.AI.Behaviors
{
    class BehaviorGoInteract : BehaviorQueue// BehaviorSequence
    {
        public BehaviorGoInteract(string targetKey, int range, Interaction interaction)
        {
            this.Children = new List<Behavior>()
            {
                new BehaviorDomain(new ConditionAll(new BehaviorTargetEntityExists(targetKey), new IsAt(targetKey, range)),
                    new BehaviorInteractionNew(targetKey, interaction)),
                new BehaviorDomain(new BehaviorTargetEntityExists(targetKey),
                    new BehaviorGetAtNewNew(targetKey))//, range))
                
                //new BehaviorDomain(new IsAt(targetKey, range),
                //    new BehaviorInteractionNew(targetKey, interaction)),
                //new BehaviorGetAt(targetKey, range)
            };
        }
        //public BehaviorGoInteract(string targetKey, int range, string interactionKey)
        //{
        //    this.Children = new List<Behavior>()
        //    {
        //        //new BehaviorGetAt(targetKey, range),
        //        //new BehaviorInteractionNew(targetKey, interaction)
        //        new BehaviorDomain(new IsAt(targetKey, range),
        //            new BehaviorInteractionNew(targetKey, interactionKey)),
        //        new BehaviorGetAtNewNew(targetKey, range)
        //    };
        //}
        public override BehaviorState Execute(Actor parent, AIState state)
        {
            return base.Execute(parent, state);
        }
        //public override object Clone()
        //{
            
        //    throw new Exception();
        //}
    }
}
