using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.Animations;

namespace Start_a_Town_
{
    public abstract class InteractionPerpetual : Interaction
    {
        Func<bool> FinishCondition;
        public InteractionPerpetual()
        {
            //this.Animation = AnimationTool.Create();
            this.Animation = new Animation(AnimationDef.Tool);// AnimationTool.Create();

        }
        public InteractionPerpetual(string name)//, int secs = 0)
            : base(name, 0)
        {
            this.RunningType = RunningTypes.Continuous;
            this.Animation = new Animation(AnimationDef.Tool);// AnimationTool.Create();

        }
        //protected virtual AnimationCollection WorkAnimation { get { return new AnimationTool(); } }
        //public override void Start(GameObject a, TargetArgs t)
        //{
        //    //this.Animation = new AnimationTool(() => OnUpdate(a, t));
        //    //this.Animation = AnimationTool.Create();
        //    //this.Animation = AnimationTool.Create(a);
        //    base.Start(a, t);
        //}
        public abstract void OnUpdate(GameObject a, TargetArgs t);
        internal override void InitAction(GameObject actor, TargetArgs target)
        {
        }
        internal override void AfterLoad(GameObject actor, TargetArgs target)
        {
            //var anitool = (this.Animation as AnimationTool);
            //anitool.Contact = () => OnUpdate(actor, target);
            //this.Animation.Entity = actor;
            base.AfterLoad(actor, target);
        }
        internal override void OnToolContact(GameObject parent, TargetArgs target)
        {
            this.OnUpdate(parent, target);
        }
    }
}
