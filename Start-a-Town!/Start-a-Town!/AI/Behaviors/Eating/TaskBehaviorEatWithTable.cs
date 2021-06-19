using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.Components.Interactions;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.AI.Behaviors
{
    class TaskBehaviorEatWithTable : BehaviorPerformTask
    {
        //public const int FoodIndex = 0;
        //public const int TableIndex = 1;

        TargetArgs Food { get { return this.Task.GetTarget(FoodInd); } }
        TargetArgs Table { get { return this.Task.GetTarget(EatingSurfaceInd); } }
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
            yield return BehaviorHelper.InteractInInventoryOrWorld(FoodInd, () => new InteractionHaul(task.GetAmount(FoodInd)));
            yield return BehaviorHelper.SetTarget(FoodInd, () =>
            {
                var carried = actor.Carried;
                var previousStack = task.GetTarget(FoodInd).Object;
                if (carried != previousStack)
                    actor.Unreserve(previousStack);
                return carried;
            });
            //var eat = new BehaviorInteractionNew(FoodInd, new Components.ConsumableComponent.InteractionConsume());
            //yield return BehaviorHelper.JumpIfTrue(eat, () => this.Table.Type == TargetType.Null);
            yield return new BehaviorGetAtNewNew(EatingSurfaceInd);
            var onTable = TargetIndex.C;
            yield return BehaviorHelper.SetTarget(onTable, Table.Global.Above());
            yield return new BehaviorInteractionNew(onTable, new UseHauledOnTarget());
            //yield return eat;
            yield return new BehaviorInteractionNew(FoodInd, new Components.ConsumableComponent.InteractionConsume());
            yield return new BehaviorInteractionNew(() => new InteractionThrow()); // in case somehow more food than necessary is been carried
        }
        //protected override IEnumerable<Behavior> GetSteps()
        //{
        //    var task = this.Task as TaskEating;
        //    var food = task.Food;
        //    var eatingPlace = task.EatingPlace;
        //    var actor = this.Actor;

        //    var inInventory = actor.InventoryContains(food.Object);
        //    var inter = inInventory ? new HaulFromInventory() : new Haul(1) as Interaction;

        //    if (!inInventory)
        //        yield return new BehaviorGetAtNewNew(FoodInd, 1);

        //    if (eatingPlace.Type == TargetType.Null)
        //    {
        //        //yield return new BehaviorInteractionNew(food, new Haul(1));
        //        yield return new BehaviorInteractionNew(FoodInd, inter);

        //    }
        //    else if (eatingPlace.Global != food.Global.SnapToBlock())
        //    {
        //        yield return new BehaviorInteractionNew(FoodInd, inter);// new Haul(1));
        //        yield return new BehaviorGetAtNewNew(EatingSurfaceInd, 1);
        //        yield return new BehaviorInteractionNew(EatingSurfaceInd, new UseHauledOnTarget());
        //    }
        //    yield return new BehaviorInteractionNew(FoodInd, new Start_a_Town_.Components.ConsumableComponent.InteractionConsume());
        //}

        protected override bool InitExtraReservations()
        {
            var tableRes = (Table.Type == TargetType.Null) ? true : Actor.Reserve(Table, 1) && Actor.Reserve(Table.Global.Above());
            return Actor.Reserve(Food, 1) && tableRes;

            //return 
            //    Actor.Reserve(Food, 1) &&
            //    Actor.Reserve(Table, 1) &&
            //    Actor.Reserve(Table.Global.Above())
            //    ;


            //return base.InitBaseReservations();
        }

        private bool IsTableSurfaceEmpty(TargetArgs table)
        {
            return !this.Actor.Map.GetObjects(table.Global).Any();
        }
        private static bool IsTableSurfaceEmpty(IMap map, Vector3 table)
        {
            return !map.GetObjects(table).Any();
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
