using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Start_a_Town_.Components.Interactions
{
    class ScriptEquip : Script
    {
        public ScriptEquip()
        {
            this.ID = Types.Equipping;
            this.Name = "Equip";
            this.BaseTimeInSeconds = 0;
           // this.RangeCheck = InventoryFirst;// DefaultRangeCheck;
            this.AddComponent(new ScriptEvaluations(this.Fail,
                new ScriptEvaluation(
                    a =>
                    {
                        return a.Target.Object.HasComponent<EquipComponent>();
                    },
                    Message.Types.InvalidTarget
                    )));
            this.AddComponent(new ScriptRangeCheck(Script.DefaultRangeCheck, InteractionOld.DefaultRange));
        }

        public override void OnStart(ScriptArgs args)
        {
            //args.Net.PostLocalEvent(args.Actor, ObjectEventArgs.Create(Message.Types.Hold, new object[] { args.Target.Object }));
            GameObjectSlot objSlot = 
                args.Target.Object.Exists ? 
                args.Target.Object.ToSlot() : 
                (from slot in args.Actor.GetChildren() where slot.HasValue select slot).FirstOrDefault(foo => foo.Object == args.Target.Object);
            //GameObjectSlot gearSlot = args.Actor.GetComponent<BodyComponent>().BodyParts[args.Target.Object.GetComponent<EquipComponent>().Slot].Wearing;
            var type = args.Target.Object.GetComponent<EquipComponent>().Type;
            GameObjectSlot gearSlot = GearComponent.GetSlot(args.Actor, type);// args.Actor.GetComponent<GearComponent>().EquipmentSlots[args.Target.Object.GetComponent<EquipComponent>().Type];

            //SwapWithCurrent(args, objSlot, gearSlot);

            // despawn item's entity from world
            args.Net.Despawn(objSlot.Object);

            // attempt to store current equipped item in inventory, otherwise drop it if inventory is full
            PersonalInventoryComponent.InsertOld(args.Actor, gearSlot);


            // equip new item
            //gearSlot.Set(objSlot.Object);
            gearSlot.Swap(objSlot);

            Finish(args);
        }

        private static void SwapWithCurrent(ScriptArgs args, GameObjectSlot objSlot, GameObjectSlot gearSlot)
        {
            args.Net.Despawn(objSlot.Object);
            if (gearSlot.HasValue)
                args.Net.Spawn(gearSlot.Object, objSlot.Object.Global);
            gearSlot.Swap(objSlot);
        }

        public override object Clone()
        {
            return new ScriptEquip();
        }
    }
}
