using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.Blocks;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.AI.Behaviors
{
    class BehaviorOpenDoor : Behavior
    {
        Vector3? CurrentOpenDoor;

        public override BehaviorState Execute(Actor parent, AIState state)
        {
            if (this.CurrentOpenDoor.HasValue)
            {
                var doorBox = BlockDoor.GetBoundingBox(parent.Map, this.CurrentOpenDoor.Value);
                if (!parent.Intersects(doorBox))
                {
                    // if the actor's boundingbox doesn't intersect the doors boundingdoor, close the door UNLESS IT'S OBSTRUCTED
                    if (!parent.Map.GetObjectsLazy().Any(o=>o.Intersects(doorBox)))
                        parent.Interact(new InteractionToggleDoor(), this.CurrentOpenDoor.Value);
                    // if the door is obstructed, dont retry to close it?
                    this.CurrentOpenDoor = null;
                    return BehaviorState.Fail;
                }
            }
            var nextStep = parent.GetNextStep();
            var doors = parent.Global.SnapToBlock().GetNeighborsSameZ().Where(adj => parent.Intersects(nextStep, BlockDoor.GetBoundingBox(parent.Map, adj)));
            if (!doors.Any())
                return BehaviorState.Fail;

            var door = doors.First();
            var cell = parent.Map.GetCell(door);
            if (cell == null)
                return BehaviorState.Fail;
            if(cell.Block is not BlockDoor)
                return BehaviorState.Fail;
            var doorstate = BlockDoor.GetState(cell.BlockData);
            this.CurrentOpenDoor = door; // IF IT'S ALREADY OPEN, STORE IT ANYWAY SO THAT THE ACTOR CLOSES IT WHEN HE LEAVES THE CELL

            if (doorstate.Open) // IF IT'S ALREADY OPEN, STORE IT ANYWAY SO THAT THE ACTOR CLOSES IT WHEN HE LEAVES THE CELL
                return BehaviorState.Fail;
            
            var openInteraction = new InteractionToggleDoor();
            parent.Interact(openInteraction, door);
            //this.CurrentOpenDoor = door; // IF IT'S ALREADY OPEN, STORE IT ANYWAY SO THAT THE ACTOR CLOSES IT WHEN HE LEAVES THE CELL
            return BehaviorState.Fail;
        }

        public override object Clone()
        {
            throw new NotImplementedException();
        }
    }
}
