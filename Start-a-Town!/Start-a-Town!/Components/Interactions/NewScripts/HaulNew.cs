﻿using System;
using Start_a_Town_.Animations;

namespace Start_a_Town_.Components.Interactions
{
    class HaulNew : Interaction
    {
        int Amount;
        public HaulNew()
            : base("Haul", .4f)
        {
            this.Amount = -1;
        }
        public HaulNew(int amount = -1)
            : base("Haul", .4f)
        {
            this.Amount = amount;
        }

        public override string ToString()
        {
            return "Haul " + (this.Amount == -1 ? " All" : " x" + this.Amount.ToString());
        }
       
        public override void Start(GameObject a, TargetArgs t)
        {
            this.Animation = new Animation(AnimationDef.TouchItem);
            a.CrossFade(this.Animation, false, 25);
        }
        
        public override void Perform(GameObject actor, TargetArgs target)
        {
            switch (target.Type)
            {
                case TargetType.Position:
                    // check if hauling and drop at target position
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


                case TargetType.Slot:
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
                        if (actor.GetComponent<HaulComponent>().Carry(actor, target.Slot))
                            target.Slot.Clear();
                    }
                    break;

               
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
