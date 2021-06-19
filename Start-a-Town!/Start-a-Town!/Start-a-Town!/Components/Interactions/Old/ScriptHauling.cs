using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.Components.Interactions
{
    class ScriptHauling : Script
    {
        public override Script.Types ID
        {
            get
            {
                return Types.Hauling;
            }
        }
        public override string Name
        {
            get
            {
                return "Hauling";
            }
        }

        GameObjectSlot Watchedslot { get; set; }

        public ScriptHauling()
        {
            this.AddComponent(new ScriptRangeCheck(Script.DefaultRangeCheck, InteractionOld.DefaultRange));
            //this.AddComponent(new ScriptAnimation(Graphics.AnimationCollection.Hauling));
            this.AddComponent(new ScriptEvaluations(this.Fail,
                new ScriptEvaluation(
                    a =>
                    {
                        return a.Target.Object.HasComponent<PhysicsComponent>();
                    },
                    Message.Types.InvalidTarget
                    ),
                new ScriptEvaluation(
                    a => a.Target.Object.GetPhysics().Size >= 0,
                    Message.Types.InvalidTarget
                    )));
        }

        public override void Update(Net.IObjectProvider net, GameObject parent, Chunk chunk = null)
        {
            base.Update(net, parent, chunk);
            //if (!this.ArgsSnapshot.Actor.GetComponent<PersonalInventoryComponent>().Holding.HasValue)
            if (!this.Watchedslot.HasValue)
                this.ArgsSnapshot.Actor.GetComponent<ControlComponent>().FinishScript(this);
        }

        public override void OnStart(ScriptArgs args)
        {
            //args.Net.PostLocalEvent(args.Actor, ObjectEventArgs.Create(Message.Types.Hold, new object[] { args.Target.Object }));
            GameObjectSlot objSlot = args.Target.Object.Exists ? args.Target.Object.ToSlot() : (from slot in args.Actor.GetChildren() where slot.HasValue select slot).FirstOrDefault(foo => foo.Object == args.Target.Object);// PersonalInventoryComponent.GetFirstOrDefault(parent, foo => foo == obj);

            throw new NotImplementedException();
            //args.Actor.GetComponent<GearComponent>().Carry(args.Net, args.Actor, objSlot);
            //this.Watchedslot = args.Actor.GetComponent<GearComponent>().EquipmentSlots[GearType.Hauling];

        }
        //public override void OnStart(ScriptArgs args)
        //{
        //    //args.Net.PostLocalEvent(args.Actor, ObjectEventArgs.Create(Message.Types.Hold, new object[] { args.Target.Object }));
        //    GameObjectSlot objSlot = args.Target.Object.Exists ? args.Target.Object.ToSlot() : (from slot in args.Actor.GetChildren() where slot.HasValue select slot).FirstOrDefault(foo => foo.Object == args.Target.Object);// PersonalInventoryComponent.GetFirstOrDefault(parent, foo => foo == obj);
        //    args.Actor.GetComponent<GearComponent>().Hold(args.Net, args.Actor, objSlot);
        //    this.Watchedslot = args.Actor.GetComponent<GearComponent>().Holding;
        //    //args.Actor.GetComponent<PersonalInventoryComponent>().Holding.Object = args.Target.Object;
        //    //args.Net.Despawn(args.Target.Object);
            
        //}
        //public override void Finish(ScriptArgs args)
        //{
        //    base.Finish(args);
        //    GameObjectSlot objSlot = args.Actor.GetComponent<PersonalInventoryComponent>().Holding;
        //    GameObject parent = args.Actor;
        //    //args.Net.Spawn(objSlot.Take(), new Position(parent.Global + parent.GetPhysics().Height * Vector3.UnitZ, parent.Velocity));
        //    //objSlot.Clear();
        //}
        public override void Interrupt(ScriptArgs args)
        {
            //base.Interrupt(args);
        }
        public override object Clone()
        {
            return new ScriptHauling();
        }
    }
}
