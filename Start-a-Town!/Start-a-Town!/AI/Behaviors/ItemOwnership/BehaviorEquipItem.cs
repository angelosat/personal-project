using System.Collections.Generic;

namespace Start_a_Town_.AI.Behaviors.ItemOwnership
{
    class BehaviorEquipItem : BehaviorYield
    {
        public override string Name => "Equipping item";
        TaskEquip Task;
        public BehaviorEquipItem(TaskEquip task)
        {
            this.Task = task;
        }
        
        protected override IEnumerable<Behavior> GetCurrentStep(Actor parent, AIState state)
        {
            yield return new GetParams(this.Task);
            yield return new BehaviorMoveTo("target", 1);
            yield return new BehaviorInteractionNew("target", new Equip());
        }
        
        class GetParams : Behavior
        {
            TaskEquip Task;

            public GetParams(TaskEquip task)
            {
                this.Task = task;
            }
            public override BehaviorState Execute(Actor parent, AIState state)
            {
                state["target"] = new TargetArgs(this.Task.Item);
                return BehaviorState.Success;
            }
            public override object Clone()
            {
                return new GetParams(this.Task);
            }
        }
    }
}
