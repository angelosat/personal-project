using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.Components.Interactions;
using Start_a_Town_.AI.Behaviors;

namespace Start_a_Town_.AI
{
    class BehaviorPickUpItemNew : BehaviorPerformTask
    {
        public override string Name
        {
            get
            {
                return "Picking up item";
            }
        }

        protected override IEnumerable<Behavior> GetSteps()
        {
            var item = this.Task.TargetA;
            yield return new BehaviorGetAtNewNew(item);
            yield return new BehaviorInteractionNew(item, new InteractionHaul());
            yield return new BehaviorInteractionNew(item, new InteractionStoreHauled());
        }

        public override bool HasFailedOrEnded()
        {
            return false;
            //return !this.Actor.GetPossesions().Contains(this.Task.TargetA.Object);
        }
        protected override bool InitExtraReservations()
        {
            return this.Actor.Reserve(this.Task.TargetA);
        }
    }
}
