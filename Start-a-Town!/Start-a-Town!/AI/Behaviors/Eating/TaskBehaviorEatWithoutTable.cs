using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.Components.Interactions;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.AI.Behaviors
{
    class TaskBehaviorEatWithoutTable : BehaviorPerformTask
    {
        //public const int FoodIndex = 0;
        //public const int TableIndex = 1;

        TargetArgs Food { get { return this.Task.GetTarget(FoodInd); } }
        public const TargetIndex FoodInd = TargetIndex.A, EatingSurfaceInd = TargetIndex.B;

        public override string Name
        {
            get
            {
                return "Eating";
            }
        }
        protected override IEnumerable<Behavior> GetSteps()
        {
            var actor = this.Actor;
            var task = this.Task;
            yield return BehaviorHelper.InteractInInventoryOrWorld(FoodInd, () => new InteractionHaul(task.GetAmount(FoodInd))); //));// 
            yield return BehaviorHelper.SetTarget(FoodInd, () =>
            {
                var carried = actor.Carried;
                var previousStack = task.GetTarget(FoodInd).Object;
                if (carried != previousStack)
                    actor.Unreserve(previousStack);
                return carried;
            });
            yield return new BehaviorInteractionNew(FoodInd, new Components.ConsumableComponent.InteractionConsume());
            yield return new BehaviorInteractionNew(() => new InteractionThrow());
        }
        

        protected override bool InitExtraReservations()
        {
            return Actor.Reserve(Food, 1);// && tableRes;
        }

        //class BehaviorIsTableValid : BehaviorCondition
        //{
        //    string TableKey;
        //    public BehaviorIsTableValid(string tableKey)
        //    {
        //        this.TableKey = tableKey;
        //    }
        //    public override BehaviorState Execute(Actor parent, AIState state)
        //    {
        //        if (!state.Blackboard.ContainsKey(this.TableKey))
        //            return BehaviorState.Fail;
        //        var tableTarget = state[this.TableKey] as TargetArgs;
        //        if (tableTarget == null)
        //            return BehaviorState.Fail;
        //        var tableGlobal = tableTarget.Global - Vector3.UnitZ; //because we stored the global above the table
        //        var cell = parent.Map.GetCell(tableGlobal);
        //        if (cell.Block != Block.Stool)
        //            return BehaviorState.Fail;
        //        return BehaviorState.Success;
        //    }
        //    public override object Clone()
        //    {
        //        return new BehaviorIsTableValid(this.TableKey);
        //    }
        //}
    }

}
