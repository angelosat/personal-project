using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.Components.Interactions;

namespace Start_a_Town_.AI.Behaviors.ItemOwnership
{
    class BehaviorEquipItem : BehaviorYield
    {
        public override string Name
        {
            get
            {
                return "Equipping item";
            }
        }
        TaskEquip Task;
        //IEnumerator<Behavior> Steps;
        public BehaviorEquipItem(TaskEquip task)
        {
            this.Task = task;
            //this.Steps = GetCurrentStep().GetEnumerator();
            //this.Steps.MoveNext();
        }
        //public override BehaviorState Execute(Entity parent, AIState state)
        //{
        //    var current = this.Steps.Current;
        //    if(current!=null)
        //    {
        //        var result = current.Execute(parent, state);
        //        switch(result)
        //        {
        //            case BehaviorState.Running:
        //                return BehaviorState.Running;
        //                break;

        //            case BehaviorState.Success:
        //                var hasNext = this.Steps.MoveNext();
        //                if (!hasNext)
        //                    return BehaviorState.Success;
        //                return BehaviorState.Running;
        //                break;

        //            case BehaviorState.Fail:
        //                return BehaviorState.Fail;
        //        }
        //    }
        //    return BehaviorState.Success;
        //}

        //IEnumerable<BehaviorState> ExecuteStep(GameObject parent, AIState state)
        //{
        //    foreach (var step in this.GetCurrentStep())
        //    {
        //        //BehaviorState result = step.Execute(parent, state);
        //        BehaviorState result;
        //        do
        //        {
        //            result = step.Execute(parent, state);
        //            yield return result;
        //        } while (result == BehaviorState.Running);
        //    }
        //    yield return BehaviorState.Success;
        //}
        protected override IEnumerable<Behavior> GetCurrentStep(Actor parent, AIState state)
        {
            yield return new GetParams(this.Task);
            yield return new BehaviorMoveTo("target", 1);
            yield return new BehaviorInteractionNew("target", new Equip());
        }
        //public override object Clone()
        //{
        //    return new BehaviorEquipItem(this.Task);
        //}
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
