using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Start_a_Town_.Components.Interactions
{
    class Equip : InteractionPerpetual
    {
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

        static readonly TaskConditions conds = new TaskConditions(new AllCheck(new RangeCheck(t => t.Global, InteractionOld.DefaultRange)));
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
            GameObjectSlot objSlot =
                t.Object.Exists ?
                t.Object.ToSlot() :
                (from slot in a.GetChildren() where slot.HasValue select slot).FirstOrDefault(foo => foo.Object == t.Object);
            //GameObjectSlot gearSlot = a.GetComponent<GearComponent>().EquipmentSlots[t.Object.GetComponent<EquipComponent>().Type];
            var type = t.Object.GetComponent<EquipComponent>().Type;
            var gearSlot = GearComponent.GetSlot(a, type);
            a.Net.Despawn(objSlot.Object);
            if (gearSlot.HasValue)
                a.Net.Spawn(gearSlot.Object, objSlot.Object.Global);
            gearSlot.Swap(objSlot);
            this.State = States.Finished;
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
