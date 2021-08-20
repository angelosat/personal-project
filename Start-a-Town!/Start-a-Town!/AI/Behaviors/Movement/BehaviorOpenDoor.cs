using System;
using System.Linq;

namespace Start_a_Town_.AI.Behaviors
{
    class BehaviorOpenDoor : Behavior
    {
        IntVec3? CurrentOpenDoor;

        public override BehaviorState Execute(Actor parent, AIState state)
        {
            if (this.CurrentOpenDoor.HasValue)
            {
                if (!parent.Intersects(this.CurrentOpenDoor.Value))
                {
                    // if the actor's boundingbox doesn't intersect the doors boundingdoor, close the door UNLESS IT'S OBSTRUCTED by another entity
                    var allobj = parent.Map.GetObjectsLazy();
                    var intersecting = allobj.Where(o => o.Intersects(this.CurrentOpenDoor.Value)); // TODO not very fast
                    if (!intersecting.Any())
                        parent.Interact(new InteractionToggleDoor(), this.CurrentOpenDoor.Value);
                    // if the door is obstructed, leave it open
                    this.CurrentOpenDoor = null;
                    return BehaviorState.Fail;
                }
            }
            return HandleByCorners(parent);
        }
        private BehaviorState HandleByCorners(Actor parent)
        {
            // THE CHECKS TO OPEN OR CLOSE DOOR MUST BE THE SAME
            var corners = parent.GetBoundingBoxCorners(parent.Global + parent.Velocity);
            var map = parent.Map;
            foreach (var i in corners)
            {
                var door = i.ToCell();
                var cell = map.GetCell(door);
                if (cell == null) // why check this? is actor at the edge of map? departing?
                    continue;
                if (cell.Block is not BlockDoor)
                    continue;
                var (open, locked) = BlockDoor.GetState(cell.BlockData);
                this.CurrentOpenDoor = door; // IF IT'S ALREADY OPEN, STORE IT ANYWAY SO THAT THE ACTOR CLOSES IT WHEN HE LEAVES THE CELL

                if (open) // IF IT'S ALREADY OPEN, STORE IT ANYWAY SO THAT THE ACTOR CLOSES IT WHEN HE LEAVES THE CELL
                    continue;

                var openInteraction = new InteractionToggleDoor();
                parent.Interact(openInteraction, door);
                return BehaviorState.Fail;
            }
            return BehaviorState.Fail;
        }

        [Obsolete]
        private BehaviorState HandleByNextStep(Actor parent)
        {
            var map = parent.Map;
            var door = parent.NextCell;
            var cell = map.GetCell(door);
            if (cell == null) // why check this? is actor at the edge of map? departing?
                return BehaviorState.Fail;
            if (cell.Block is not BlockDoor)
                return BehaviorState.Fail;
            var (open, locked) = BlockDoor.GetState(cell.BlockData);
            this.CurrentOpenDoor = door; // IF IT'S ALREADY OPEN, STORE IT ANYWAY SO THAT THE ACTOR CLOSES IT WHEN HE LEAVES THE CELL

            if (open) // IF IT'S ALREADY OPEN, STORE IT ANYWAY SO THAT THE ACTOR CLOSES IT WHEN HE LEAVES THE CELL
                return BehaviorState.Fail;

            var openInteraction = new InteractionToggleDoor();
            parent.Interact(openInteraction, door);
            return BehaviorState.Fail;
        }

        public override object Clone()
        {
            throw new NotImplementedException();
        }
    }
}
