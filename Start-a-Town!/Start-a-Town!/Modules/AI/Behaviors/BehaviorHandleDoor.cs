using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components.AI;
using Start_a_Town_.Components.Interactions;
using Start_a_Town_.Blocks;
using Start_a_Town_.Net;

namespace Start_a_Town_.AI.Behaviors
{
    class BehaviorHandleDoor : Behavior
    {
        class DoorMemory
        {
            public Vector3 EntryOrigin;
            public TargetArgs DoorTarget;
            public float Timer;
            static float TimerMax = Engine.TicksPerSecond * 60;
            public DoorMemory(Vector3 entryOrigin, TargetArgs door)
            {
                this.EntryOrigin = entryOrigin;
                this.DoorTarget = door;
                this.Timer = TimerMax;
            }

            internal void Update()
            {
                this.Timer--;
            }
        }
        InteractionToggleDoor Interaction;
        TargetArgs DoorTarget;
        //TargetArgs EntryOrigin;
        Dictionary<Vector3, DoorMemory> Entries = new Dictionary<Vector3, DoorMemory>();
        //HashSet<Vector3> Entries = new HashSet<Vector3>();
        public override BehaviorState Execute(Actor parent, AIState state)
        {
            UpdateMemories();

            if (this.DoorTarget == null)
                return BehaviorState.Fail;

            if(this.Interaction== null)
            {
                this.Interaction = new InteractionToggleDoor();
                var server = parent.Net as Server;
                server.AIHandler.AIInteract(parent, this.Interaction, this.DoorTarget);
                return BehaviorState.Running;
            }

            switch (this.Interaction.State)
            {
                case Start_a_Town_.Interaction.States.Running:
                    return BehaviorState.Running;

                case Start_a_Town_.Interaction.States.Finished:
                    var entry = parent.Global.Round();
                    this.Entries[entry] = new DoorMemory(entry, this.DoorTarget);
                    this.DoorTarget = null;
                    this.Interaction = null;
                    AIManager.AIStartMove(parent);
                    return BehaviorState.Fail;

                default:
                    break;
            }

            return BehaviorState.Fail;
        }

        private void UpdateMemories()
        {
            foreach (var mem in this.Entries.Values.ToList())
            {
                mem.Update();
                if (mem.Timer <= 0)
                    this.Entries.Remove(mem.EntryOrigin);
            }
        }
        //public override bool HandleMessage(GameObject parent, ObjectEventArgs e)
        //{
        //    switch(e.Type)
        //    {
        //        case Components.Message.Types.BlockCollision:
        //            var vec = (Vector3)e.Parameters[0];
        //            var block = parent.Map.GetBlock(vec);
        //            if (block != Block.Door)
        //                break;
        //            AIManager.AIStopMove(parent);
        //            this.DoorTarget = new TargetArgs(vec);
        //            //var entry = parent.Global.Round();
        //            //this.Entries[entry] = new DoorMemory(entry, this.DoorTarget);
        //            // TODO: create the entry when the interaction is started or completed, instead of here?
        //            break;

        //        case Components.Message.Types.EntityMovedCell:
        //            if (this.Entries.Count > 0)
        //            {
        //                var prev = (Vector3)e.Parameters[0];
        //                var next = (Vector3)e.Parameters[1];
        //                DoorMemory found;
        //                if (!this.Entries.TryGetValue(next, out found))
        //                    break;
        //                AIManager.AIStopMove(parent);
        //                this.Entries.Remove(next);
        //                this.DoorTarget = found.DoorTarget;
        //            }
        //            break;

        //        default:
        //            break;
        //    }
        //    return true;
        //}
        public override object Clone()
        {
            return new BehaviorHandleDoor();
        }
    }
}
