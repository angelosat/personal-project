using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.Graphics.Animations;
using Start_a_Town_.Graphics;

namespace Start_a_Town_.Components.Interactions
{
    abstract class InteractionPerpetual : Interaction
    {
        Func<bool> FinishCondition;

        public InteractionPerpetual(string name)//, int secs = 0)
            : base(name, 0)
        {
            this.Animation = this.WorkAnimation;// new AnimationTool();
            this.RunningType = RunningTypes.Continuous;
        }
        protected virtual AnimationCollection WorkAnimation { get { return new AnimationTool(); } }
        public override void Start(GameObject a, TargetArgs t)
        {
            (this.Animation as AnimationTool).Contact = () => OnUpdate(a, t);
            base.Start(a, t);
        }

        public abstract void OnUpdate(GameObject a, TargetArgs t);
    }
}
