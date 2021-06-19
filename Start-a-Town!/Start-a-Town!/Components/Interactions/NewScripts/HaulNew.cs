using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.Animations;

namespace Start_a_Town_.Components.Interactions
{
    class HaulNew : Interaction
    {
        int Amount;
        public HaulNew()
            : base("Haul", .4f)
        {
            //this.Animation = new Graphics.Animations.AnimationPlaceItem();
            //this.Animation = AnimationPlaceItem.PlaceItem();
            this.Amount = -1;
        }
        public HaulNew(int amount = -1)
            : base("Haul", .4f)
        {
            //this.Animation = new Graphics.Animations.AnimationPlaceItem();
            //this.Animation = AnimationPlaceItem.PlaceItem();
            this.Amount = amount;
        }

        //static readonly ScriptTaskCondition Cancel = new Exists();
        //static readonly TaskConditions Conds = 
        //    new TaskConditions(
        //        new AllCheck(
        //            new RangeCheck(RangeCheck.DefaultRange),
        //            new AnyCheck(
        //                new TargetTypeCheck(TargetType.Position),
        //                new AllCheck(
        //                    new TargetTypeCheck(TargetType.Entity),
        //                    new Exists() ),
        //                new TargetTypeCheck(TargetType.Slot)))
        //    );
      
        public override string ToString()
        {
            return "Haul " + (this.Amount == -1 ? " All" : " x" + this.Amount.ToString());
        }
        //public override TaskConditions Conditions
        //{
        //    get 
        //    { 
        //        return Conds;
        //    }
        //}
        //public override ScriptTaskCondition CancelState
        //{
        //    get
        //    {
        //        return Cancel;
        //    }
        //}
        public override void Start(GameObject a, TargetArgs t)
        {
            //this.Animation = AnimationPlaceItem.PlaceItem(a);
            this.Animation = new Animation(AnimationDef.TouchItem);

            //a.Body.CrossFade(this.Animation, false, 25);
            a.CrossFade(this.Animation, false, 25);
        }
        //protected override void Stop(GameObject actor)
        //{
        //    //actor.Body.FadeOutAnimation(this.Animation, 100 / Engine.TargetFps);
        //}
        public override void Perform(GameObject actor, TargetArgs target)
        {
            switch (target.Type)
            {
                case TargetType.Position:
                    // check if hauling and drop at target position
                    //GameObject held = actor.GetComponent<GearComponent>().Holding.Take();
                    GameObject held = actor.GetComponent<HaulComponent>().Holding.Take();

                    if (held == null)
                        return;
                    actor.Net.Spawn(held, target.FinalGlobal);
                    break;

                    // new: if inventoryable insert to inventory, if carryable carry
                    // dont carry inventoriables (test)
                case TargetType.Entity:


                    // error handle!!!
                    if (this.Amount > target.Object.StackSize)
                        throw new Exception();
                    PersonalInventoryComponent.PickUpNewNew(actor, target.Object, this.Amount == - 1 ? target.Object.StackSize : this.Amount);

                    break;

                    //var hauling = actor.GetComponent<HaulComponent>().GetSlot();//.Slot;

                    //if (hauling.HasValue)
                    //{
                    //    // if hauling is the same object type as target, combine them in the carried stack
                    //    var hauled = hauling.Object;
                    //    if (hauled.ID == target.Object.ID)
                    //    {
                    //        var transferAmount = Math.Min(target.Object.StackSize, hauled.StackMax - hauled.StackSize);
                    //        hauled.StackSize += transferAmount;
                    //        if (transferAmount == target.Object.StackSize)
                    //        {
                    //            actor.Net.Despawn(target.Object);
                    //            actor.Net.DisposeObject(target.Object);
                    //        }
                    //        break;
                    //    }
                    //    else // if already hauling a different item type, store existing hauled in backpack (TODO: or drop it if backpack full), and haul new item
                    //    {
                    //        //actor.GetComponent<PersonalInventoryComponent>().Insert(actor, hauling);
                    //        PersonalInventoryComponent.PickUp(actor, target.Object.ToSlot());

                    //        //actor.GetComponent<HaulComponent>().Carry(actor, target.Object.ToSlot());

                    //    }
                    //    // this line makes actor drop current carried object and carry target object
                    //    //actor.Net.PostLocalEvent(target.Object, Message.Types.Insert, hauling);
                    //    break;
                    //}
                    //else
                    //    //actor.GetComponent<HaulComponent>().Slot.Object = target.Object;
                    //    actor.GetComponent<HaulComponent>().Carry(actor, target.Object.ToSlot());

                    ////actor.Net.PostLocalEvent(actor, Message.Types.Insert, target.Object.ToSlot());
                    ////actor.GetComponent<PersonalInventoryComponent>().Insert(target.Object.ToSlot());




                    //// TRYING TO MAKE HAULABLE ITEMS INVENTORYABLE
                    ////if (target.Object.GetPhysics().Size == ObjectSize.Inventoryable)
                    ////    actor.Net.PostLocalEvent(actor, Message.Types.Insert, target.Object.ToSlot());
                    ////else if (target.Object.GetPhysics().Size == ObjectSize.Haulable)
                    ////    actor.GetComponent<HaulComponent>().Carry(actor, target.Object.ToSlot());

                    //break;

                case TargetType.Slot:
                    //hauling = actor.GetComponent<GearComponent>().EquipmentSlots[GearType.Hauling];
                    var hauling = actor.GetComponent<HaulComponent>().GetSlot();//.Slot;

                    if (hauling.HasValue)
                    {
                        actor.Net.PostLocalEvent(target.Object, Message.Types.Insert, hauling);
                        break;
                    }
                    var obj = target.Slot.Object;
                    if (obj.Physics.Size == ObjectSize.Inventoryable)
                        actor.Net.PostLocalEvent(actor, Message.Types.Insert, target.Slot);
                    else if (obj.Physics.Size == ObjectSize.Haulable)
                    {
                        //if (actor.GetComponent<GearComponent>().Carry(actor, target.Slot))
                        if (actor.GetComponent<HaulComponent>().Carry(actor, target.Slot))
                            target.Slot.Clear();
                    }
                    break;

                //    // old
                //case TargetType.Entity:
                //    var hauling = actor.GetComponent<GearComponent>().EquipmentSlots[GearType.Hauling];
                //    if (hauling.HasValue)
                //    {
                //        actor.Net.PostLocalEvent(target.Object, Message.Types.Insert, hauling);
                //        break;
                //    }
                //    if (target.Object.GetPhysics().Size == ObjectSize.Inventoryable)
                //        actor.Net.PostLocalEvent(actor, Message.Types.Insert, target.Object.ToSlot());
                //    break;

                default:
                    break;
            }
        }

        public override object Clone()
        {
            return new HaulNew(this.Amount);
        }

        protected override void WriteExtra(System.IO.BinaryWriter w)
        {
            w.Write(this.Amount);
        }
        protected override void ReadExtra(System.IO.BinaryReader r)
        {
            this.Amount = r.ReadInt32();
        }
        protected override void AddSaveData(SaveTag tag)
        {
            tag.Add(this.Amount.Save("Amount"));
        }
        public override void LoadData(SaveTag tag)
        {
            tag.TryGetTagValue<int>("Amount", out this.Amount);
        }
    }
}
