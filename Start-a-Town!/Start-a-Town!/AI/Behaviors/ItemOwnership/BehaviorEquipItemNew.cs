using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.Components.Interactions;

namespace Start_a_Town_.AI.Behaviors
{
    class BehaviorEquipItemNew : BehaviorPerformTask
    {
        public override string Name
        {
            get
            {
                return "Equipping item";
            }
        }
        public BehaviorEquipItemNew()
        {

        }
        public BehaviorEquipItemNew(AITask task)
        {
            this.Task = task;
        }
        
        protected override IEnumerable<Behavior> GetSteps()
        {
            yield return new BehaviorMoveTo(this.Task.TargetA, 1);
            yield return new BehaviorInteractionNew(this.Task.TargetA, new Equip());
        }
        protected override bool InitExtraReservations()
        {
            return this.Actor.Reserve(this.Task.TargetA, 1);
        }
    }
}
