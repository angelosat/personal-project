using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_.Components.Interactions
{
    class PickUp : Interaction
    {
        public PickUp()
            : base("PickUp", .4f)
        {
            this.Animation = new Graphics.Animations.AnimationPlaceItem();
            //this.CancelState = new Exists();
        }

        static readonly ScriptTaskCondition Cancel = new Exists();
        static readonly TaskConditions Conds = 
            new TaskConditions(
                new AllCheck(
                    new RangeCheck(RangeCheck.DefaultRange),
                    new AnyCheck(
                        new TargetTypeCheck(TargetType.Position),
                        new AllCheck(
                            new TargetTypeCheck(TargetType.Entity),
                            new Exists() ),
                        new TargetTypeCheck(TargetType.Slot)))
            );

        public override TaskConditions Conditions
        {
            get 
            { 
                return Conds;
            }
        }
        public override ScriptTaskCondition CancelState
        {
            get
            {
                return Cancel;
            }
        }
        public override void Start(GameObject a, TargetArgs t)
        {
            base.Start(a, t);
            //var haul = a.GetComponent<HaulComponent>();
            //a.Body.FadeOutAnimation(haul.Animation, this.Seconds / 2f);// 1f);
            //a.Body.Start(this.Animation);
            a.Body.CrossFade(this.Animation, false, 25);
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

                    if (held.IsNull())
                        return;
                    actor.Net.Spawn(held, target.FinalGlobal);
                    break;

                    // new: if inventoryable insert to inventory, if carryable carry
                    // dont carry inventoriables (test)
                case TargetType.Entity:
                    //actor.GetComponent<PersonalInventoryComponent>().PickUp(actor, target.Object.ToSlot());
                    PersonalInventoryComponent.PickUp(actor, target.Object.ToSlot());
                    break;

                    var hauling = actor.GetComponent<HaulComponent>().GetSlot();//.Slot;

                    if (hauling.HasValue)
                    {
                        // if hauling is the same object type as target, combine them in the carried stack
                        var hauled = hauling.Object;
                        if (hauled.ID == target.Object.ID)
                        {
                            var transferAmount = Math.Min(target.Object.StackSize, hauled.StackMax - hauled.StackSize);
                            hauled.StackSize += transferAmount;
                            if (transferAmount == target.Object.StackSize)
                            {
                                actor.Net.Despawn(target.Object);
                                actor.Net.DisposeObject(target.Object);
                            }
                            break;
                        }
                        else // if already hauling a different item type, store existing hauled in backpack (TODO: or drop it if backpack full), and haul new item
                        {
                            //actor.GetComponent<PersonalInventoryComponent>().Insert(actor, hauling);
                            PersonalInventoryComponent.PickUp(actor, target.Object.ToSlot());

                            //actor.GetComponent<HaulComponent>().Carry(actor, target.Object.ToSlot());

                        }
                        // this line makes actor drop current carried object and carry target object
                        //actor.Net.PostLocalEvent(target.Object, Message.Types.Insert, hauling);
                        break;
                    }
                    else
                        //actor.GetComponent<HaulComponent>().Slot.Object = target.Object;
                        actor.GetComponent<HaulComponent>().Carry(actor, target.Object.ToSlot());

                    //actor.Net.PostLocalEvent(actor, Message.Types.Insert, target.Object.ToSlot());
                    //actor.GetComponent<PersonalInventoryComponent>().Insert(target.Object.ToSlot());




                    // TRYING TO MAKE HAULABLE ITEMS INVENTORYABLE
                    //if (target.Object.GetPhysics().Size == ObjectSize.Inventoryable)
                    //    actor.Net.PostLocalEvent(actor, Message.Types.Insert, target.Object.ToSlot());
                    //else if (target.Object.GetPhysics().Size == ObjectSize.Haulable)
                    //    actor.GetComponent<HaulComponent>().Carry(actor, target.Object.ToSlot());

                    break;

                case TargetType.Slot:
                    //hauling = actor.GetComponent<GearComponent>().EquipmentSlots[GearType.Hauling];
                    hauling = actor.GetComponent<HaulComponent>().GetSlot();//.Slot;

                    if (hauling.HasValue)
                    {
                        actor.Net.PostLocalEvent(target.Object, Message.Types.Insert, hauling);
                        break;
                    }
                    var obj = target.Slot.Object;
                    if (obj.GetPhysics().Size == ObjectSize.Inventoryable)
                        actor.Net.PostLocalEvent(actor, Message.Types.Insert, target.Slot);
                    else if (obj.GetPhysics().Size == ObjectSize.Haulable)
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
            return new PickUp();
        }
    }
}
