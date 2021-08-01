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
        
        public abstract void OnUpdate(Actor a, TargetArgs t);
        internal override void InitAction(Actor actor, TargetArgs target)
        {
        }
        internal override void AfterLoad(Actor actor, TargetArgs target)
        {
            base.AfterLoad(actor, target);
        }
        internal override void OnToolContact(Actor parent, TargetArgs target)
        {
            this.OnUpdate(parent, target);
        }
    }
}
