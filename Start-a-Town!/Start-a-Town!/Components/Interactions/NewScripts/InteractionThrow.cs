﻿using Microsoft.Xna.Framework;
using Start_a_Town_.Components;

namespace Start_a_Town_
{
    class InteractionThrow : Interaction
    {
        bool All;
        public InteractionThrow():this(true)
        {

        }
        public InteractionThrow(bool all)
            : base(
            "Throw",
            0)
        {
            this.All = all;
        }

        static readonly TaskConditions conds = new(new AllCheck(
                new ScriptTaskCondition("IsCarrying", Test, Message.Types.InteractionFailed)));
        public override TaskConditions Conditions => conds;

        static bool Test(GameObject a, TargetArgs target)
        {
            return a.GetComponent<HaulComponent>().GetObject() != null;
        }

        public override bool AvailabilityCondition(GameObject actor, TargetArgs target)
        {
            return actor.GetComponent<HaulComponent>().GetObject() != null;
        }

        internal override void InitAction(GameObject actor, TargetArgs target)
        {
            base.InitAction(actor, target);
            
            var slot = actor.Inventory.GetHauling();
            var obj = slot.Object;
            if (obj == null)
                return;
            var all = this.All;
            GameObject newobj = all ? obj : obj.TrySplitOne();
            var velocity = new Vector3(target.Direction, 0) * 0.1f + actor.Velocity;

            /// GLOBAL DOESNT GET SET HERE BECAUSE THE OBJ STILL HAVE THE ACTOR AS THE PARENT AND RETURNS HIS GLOBAL
            //newobj.Global = actor.Global + new Vector3(0, 0, actor.Physics.Height); 
            ///
            var newGlobal = actor.Global + new Vector3(0, 0, actor.Physics.Height);
            newobj.Velocity = velocity;
            newobj.Physics.Enabled = true;
            if (newobj != obj)
            {
                if (actor.Net is Net.Server server)
                {
                    newobj.SyncInstantiate(server);
                    actor.Map.SyncSpawn(newobj, newGlobal, velocity);
                }
            }
            else
            {
                newobj.Spawn(actor.Map, newGlobal);
            }
            if (obj == newobj)
                slot.Clear();
        }

        // TODO: make it so i have access to the carried item's stacksize, and include it in the name ( Throw 1 vs Throw 16 for example)
        public override string ToString()
        {
            return this.Name + (this.All ? " All" : "");
        }

        public override object Clone()
        {
            return new InteractionThrow(this.All);
        }
        protected override void WriteExtra(System.IO.BinaryWriter w)
        {
            w.Write(this.All);
        }
        protected override void ReadExtra(System.IO.BinaryReader r)
        {
            this.All = r.ReadBoolean();
        }
    }
}
