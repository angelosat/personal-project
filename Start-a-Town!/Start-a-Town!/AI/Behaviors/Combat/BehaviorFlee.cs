using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components;

namespace Start_a_Town_.AI.Behaviors
{
    class BehaviorFlee : Behavior
    {
        public override BehaviorState Execute(Actor parent, AIState state)
        {
            var threat = state.Threats.FirstOrDefault();
            var entity = threat.Entity;
            var diff = parent.Global - entity.Global;
            var dir = diff.XY();
            dir.Normalize();
            //parent.Direction = new Vector3(dir.X, dir.Y, 0);
            AIManager.AIMove(parent, new Vector3(dir.X, dir.Y, 0));
            // TODO: stop fleeing after certain amount of range. here? or separate behavior?
            return BehaviorState.Success;
            //throw new NotImplementedException();
        }
        public override object Clone()
        {
            return new BehaviorFlee();
        }
    }
    //class BehaviorFlee : BehaviorSequence
    //{
    //    public BehaviorFlee()
    //    {
    //        this.Children = new List<Behavior>()
    //        {
    //            new BehaviorResourceCheck(Resource.Types.Health, BehaviorResourceCheck.Comparison.Less, .5f)
    //        };
    //    }
    //    //public override BehaviorState Execute(Entity parent, AIState state)
    //    //{
    //    //    var health = ResourcesComponent.GetResource(parent, Resource.Types.Health);
    //    //    throw new NotImplementedException();
    //    //}
    //    //public override object Clone()
    //    //{
    //    //    return new BehaviorFlee();
    //    //}
    //}
}
