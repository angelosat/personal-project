namespace Start_a_Town_.AI.Behaviors
{
    class BehaviorHandleOrders : Behavior
    {
        BehaviorGetAtNewNew CurrentBehav;
        TargetArgs CurrentMoveOrder = TargetArgs.Null;
        public override BehaviorState Execute(Actor parent, AIState state)
        {
            if (this.CurrentBehav != null && this.CurrentMoveOrder != TargetArgs.Null && state.MoveOrder == this.CurrentMoveOrder)
            {
                if (this.CurrentBehav.Execute(parent, state) != BehaviorState.Running)
                {
                    this.CurrentBehav = null;
                    state.MoveOrders.Dequeue();
                    this.CurrentMoveOrder = TargetArgs.Null;
                }
                else
                    return BehaviorState.Running;
            }
            
            if (state.MoveOrder != null)
                if (state.MoveOrder.Type == TargetType.Position)
                {
                    var destination = state.MoveOrder.Global.Above();
                    if (parent.IsAt(destination))
                        return BehaviorState.Running;
                    var target = new TargetArgs(parent.Map, destination);
                    if (parent.CanReach(target))
                    {
                        this.CurrentBehav = new BehaviorGetAtNewNew(target, PathingSync.FinishMode.Exact);
                        this.CurrentMoveOrder = state.MoveOrder;
                        return BehaviorState.Running;
                    }
                }
            return BehaviorState.Fail;
        }
        public override object Clone()
        {
            return new BehaviorHandleOrders();
        }
    }
}
