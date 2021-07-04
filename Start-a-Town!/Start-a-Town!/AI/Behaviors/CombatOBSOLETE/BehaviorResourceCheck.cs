using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.Components;

namespace Start_a_Town_.AI.Behaviors
{
    class BehaviorResourceCheck : Behavior
    {
        public enum Comparison { More, MoreEqual, Less, LessEqual, Equal };
        Comparison CheckType;
        float PercentageValue;
        ResourceDef Type;
        public BehaviorResourceCheck(ResourceDef type, Comparison comp, float value)
        {
            this.CheckType = comp;
            this.Type = type;
            this.PercentageValue = value;
        }
        public override BehaviorState Execute(Actor parent, AIState state)
        {
            var health = parent.GetResource(this.Type);// ResourcesComponent.GetResource(parent, this.Type);
            var diff = health.Percentage - this.PercentageValue;
            switch(this.CheckType)
            {
                case Comparison.More:
                    return diff > 0 ? BehaviorState.Success : BehaviorState.Fail;
                case Comparison.MoreEqual:
                    return diff >= 0 ? BehaviorState.Success : BehaviorState.Fail;
                case Comparison.Less:
                    return diff < 0 ? BehaviorState.Success : BehaviorState.Fail;
                case Comparison.LessEqual:
                    return diff <= 0 ? BehaviorState.Success : BehaviorState.Fail;
                case Comparison.Equal:
                    return diff == 0 ? BehaviorState.Success : BehaviorState.Fail;
                default:
                    throw new Exception();
            }
        }
        public override object Clone()
        {
            return new BehaviorResourceCheck(this.Type, this.CheckType, this.PercentageValue);
        }
    }
}
