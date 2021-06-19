using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Start_a_Town_.Components;

namespace Start_a_Town_
{
    class Equip : InteractionPerpetual
    {
        static public int ID = "Equip".GetHashCode();

        public Equip()
            : base("Equip")
        {
            //var ani = new Graphics.Animations.AnimationPlaceItem();
            //this.Animation = ani;

        }

        //protected override Graphics.AnimationCollection WorkAnimation
        //{
        //    get
        //    {
        //        return new Graphics.Animations.AnimationPlaceItem();
        //    }
        //}
        //public override void Start(GameObject a, TargetArgs t)
        //{
        //    (this.Animation as Graphics.Animations.AnimationPlaceItem).Finish = () => OnUpdate(a, t);
        //}
        public override void Start(GameObject a, TargetArgs t)
        {
            //a.Body.CrossFade(this.Animation, false, 25);
            a.CrossFade(this.Animation, false, 25);

        }

        static readonly TaskConditions conds = new(new AllCheck(new RangeCheck(t => t.Global, Interaction.DefaultRange)));
        public override TaskConditions Conditions
        {
            get
            {
                return conds;
            }
        }
        //public override void Perform(GameObject a, TargetArgs t)
        public override void OnUpdate(GameObject a, TargetArgs t)
        {
            GearComponent.EquipToggle(a as Actor, t.Object as Entity);
            this.Finish(a, t);
            return;

            //GameObjectSlot objSlot =
            //    t.Object.IsSpawned ?
            //    t.Object.ToSlotLink() :
            //    (from slot in a.GetChildren() where slot.HasValue select slot).FirstOrDefault(foo => foo.Object == t.Object);
            //var type = t.Object.Def.GearType;// t.Object.GetComponent<EquipComponent>().Type;
            //if (type == null)
            //    throw new Exception();
            //var gearSlot = GearComponent.GetSlot(a, type);
            //a.Net.Despawn(objSlot.Object);
            //if (gearSlot.HasValue)
            //    a.Net.Spawn(gearSlot.Object, objSlot.Object.Global);
            //gearSlot.Swap(objSlot);
            ////this.State = States.Finished;
            //this.Finish(a, t);
        }

        public override object Clone()
        {
            return new Equip();
        }
    }

    //class Equip : Interaction
    //{
    //    public Equip()
    //        : base("Equip", 0)
    //    { }
    //    static readonly TaskConditions conds = new TaskConditions(new AllCheck(new RangeCheck(t => t.Global, InteractionOld.DefaultRange)));
    //    public override TaskConditions Conditions
    //    {
    //        get
    //        {
    //            return conds;
    //        }
    //    }
    //    public override void Perform(GameObject a, TargetArgs t)
    //    {
    //        GameObjectSlot objSlot = 
    //            t.Object.Exists ? 
    //            t.Object.ToSlot() : 
    //            (from slot in a.GetChildren() where slot.HasValue select slot).FirstOrDefault(foo => foo.Object == t.Object);
    //        //GameObjectSlot gearSlot = a.GetComponent<GearComponent>().EquipmentSlots[t.Object.GetComponent<EquipComponent>().Type];
    //        var type = t.Object.GetComponent<EquipComponent>().Type;
    //        var gearSlot = GearComponent.GetSlot(a, type);
    //        a.Net.Despawn(objSlot.Object);
    //        if(gearSlot.HasValue)
    //            a.Net.Spawn(gearSlot.Object, objSlot.Object.Global);
    //        gearSlot.Swap(objSlot);

    //    }

    //    public override object Clone()
    //    {
    //        return new Equip();
    //    }
    //}
}
