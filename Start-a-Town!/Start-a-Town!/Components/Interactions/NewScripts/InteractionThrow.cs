using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components;

namespace Start_a_Town_
{
    class InteractionThrow : Interaction
    {
        bool All;
        public InteractionThrow():this(true)
        {

        }
        public InteractionThrow(bool all) //=false
            : base(
            "Throw",
            0)
        {
            this.All = all;
        }

        static readonly TaskConditions conds = new(new AllCheck(
            //new ScriptTaskCondition("IsCarrying", (a, t) => GearComponent.GetSlot(a, GearType.Hauling).Object != null, Message.Types.InteractionFailed))
                new ScriptTaskCondition("IsCarrying", Test, Message.Types.InteractionFailed)));
        public override TaskConditions Conditions
        {
            get
            {
                return conds;
            }
        }

        static bool Test(GameObject a, TargetArgs target)
        {
            //return GearComponent.GetSlot(a, GearType.Hauling).Object != null;
            //return a.GetComponent<HaulComponent>().Slot.Object != null;
            return a.GetComponent<HaulComponent>().GetObject() != null;

        }

        public override bool AvailabilityCondition(GameObject actor, TargetArgs target)
        {
            //return GearComponent.GetSlot(actor, GearType.Hauling).Object != null;
            //return actor.GetComponent<HaulComponent>().Slot.Object != null;
            return actor.GetComponent<HaulComponent>().GetObject() != null;

        }

        //public override void Perform(GameObject actor, TargetArgs target)
        //{
        //    //actor.GetComponent<GearComponent>().Throw(actor, new Vector3(target.Direction, 0), this.All);
        //    actor.GetComponent<HaulComponent>().Throw(actor, new Vector3(target.Direction, 0), this.All);

        //}
        internal override void InitAction(GameObject actor, TargetArgs target)
        {
            base.InitAction(actor, target);
            //actor.GetComponent<HaulComponent>().Throw(actor, new Vector3(target.Direction, 0), this.All);

            //var slot = this.GetSlot();
            //GameObjectSlot hauling = slot;// this.Slot;
            //if (hauling.Object == null)
            //    return false;
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
            //actor.Net.Spawn(newobj);
            if (newobj != obj)
            {
                if (actor.Net is Net.Server server)
                {
                    server.SyncInstantiate(newobj);
                    //server.SyncSpawn(newobj, actor.Map, newobj.Global, newobj.Velocity);
                    actor.Map.SyncSpawn(newobj, newGlobal, velocity);

                }
            }
            else
            {
                newobj.Spawn(actor.Map, newGlobal);
            }
            if (obj == newobj)
                slot.Clear();
            //if (all)
            //    hauling.Clear();
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
