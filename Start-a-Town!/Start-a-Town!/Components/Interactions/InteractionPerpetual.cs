using System;
using Start_a_Town_.Animations;

namespace Start_a_Town_
{
    public abstract class InteractionPerpetual : Interaction
    {
        public InteractionPerpetual()
        {
            this.Animation = new Animation(AnimationDef.Tool);
        }
        public InteractionPerpetual(string name)
            : base(name, 0)
        {
            this.RunningType = RunningTypes.Continuous;
            this.Animation = new Animation(AnimationDef.Tool);
        }
        
        public abstract void OnUpdate(GameObject a, TargetArgs t);
        internal override void InitAction(GameObject actor, TargetArgs target)
        {
        }
        internal override void AfterLoad(GameObject actor, TargetArgs target)
        {
            base.AfterLoad(actor, target);
        }
        internal override void OnToolContact(GameObject parent, TargetArgs target)
        {
            this.OnUpdate(parent, target);
        }
    }
}
