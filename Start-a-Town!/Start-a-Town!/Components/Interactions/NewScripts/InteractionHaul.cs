using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.Components;
using Start_a_Town_.Animations;

namespace Start_a_Town_
{
    class InteractionHaul : InteractionPerpetual
    {
        int Amount;
        public InteractionHaul()
            : base("Haul")
        {
            this.Animation = new Animation(AnimationDef.TouchItem);

            this.Amount = -1;
        }
        public InteractionHaul(int amount)
            : base("Haul")
        {
            this.Animation = new Animation(AnimationDef.TouchItem);

            this.Amount = amount;
            if (amount <= 0)
                throw new Exception();
        }


        public override string ToString()
        {
            return "Haul " + (this.Amount == -1 ? " All" : " x" + this.Amount.ToString());
        }


        public override void Start(GameObject a, TargetArgs t)
        {
            //this.Animation = new Animation(AnimationDef.TouchItem);
            a.CrossFade(this.Animation, false, 25);
        }
        public override void OnUpdate(GameObject actor, TargetArgs target)
        //{
        //    throw new NotImplementedException();
        //}
        //public override void Perform(GameObject actor, TargetArgs target)
        {
            if (target.Object is Actor)
                throw new Exception();
            switch (target.Type)
            {
                case TargetType.Position:
                    // check if hauling and drop at target position
                    GameObject held = actor.GetComponent<HaulComponent>().Holding.Take();

                    if (held == null)
                        break;
                    //actor.Net.Spawn(held, target.FinalGlobal);
                    held.Spawn(actor.Net.Map, target.FinalGlobal);
                    break;

                // new: if inventoryable insert to inventory, if carryable carry
                // dont carry inventoriables (test)
                case TargetType.Entity:
                    if (actor.InventoryContains(target.Object))
                    {
                        PersonalInventoryComponent.HaulFromInventory(actor, target.Object, this.Amount);
                        break;
                    }
                    var containerGlobal = target.Global;
                    var prevStackSize = target.Object.StackSize;

                    if (!target.Object.IsHaulable)
                        throw new Exception();
                    if (this.Amount > target.Object.StackSize)
                        throw new Exception();
                    PersonalInventoryComponent.PickUpNewNew(actor, target.Object, this.Amount == -1 ? target.Object.StackSize : this.Amount);

                    // if target was in container, remove it from its contents
                    if (this.Amount == prevStackSize && actor.Map.GetBlockEntity(containerGlobal) is BlockBin.BlockBinEntity container)
                        container.Remove(target.Object);

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
            this.Finish(actor, target);

        }

        public override object Clone()
        {
            return new InteractionHaul(this.Amount);
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

    //class InteractionHaul : Interaction
    //{
    //    int Amount;
    //    public InteractionHaul()
    //        : base("Haul", .4f)
    //    {
    //        this.Animation = new Animation(AnimationDef.TouchItem);

    //        this.Amount = -1;
    //    }
    //    public InteractionHaul(int amount)
    //        : base("Haul", .4f)
    //    {
    //        this.Animation = new Animation(AnimationDef.TouchItem);

    //        this.Amount = amount;
    //        if (amount <= 0)
    //            throw new Exception();
    //    }

    //    //static readonly ScriptTaskCondition Cancel = new Exists();
    //    //static readonly TaskConditions Conds = 
    //    //    new TaskConditions(
    //    //        new AllCheck(
    //    //            new RangeCheck(RangeCheck.DefaultRange),
    //    //            new AnyCheck(
    //    //                new TargetTypeCheck(TargetType.Position),
    //    //                new AllCheck(
    //    //                    new TargetTypeCheck(TargetType.Entity)
    //    //                    //,
    //    //                    //new Exists() 
    //    //                    ),
    //    //                new TargetTypeCheck(TargetType.Slot)))
    //    //    );
    //    //public override TaskConditions Conditions
    //    //{
    //    //    get
    //    //    {
    //    //        return Conds;
    //    //    }
    //    //}

    //    public override string ToString()
    //    {
    //        return "Haul " + (this.Amount == -1 ? " All" : " x" + this.Amount.ToString());
    //    }


    //    public override void Start(GameObject a, TargetArgs t)
    //    {
    //        //this.Animation = new Animation(AnimationDef.TouchItem);
    //        a.CrossFade(this.Animation, false, 25);
    //    }

    //    public override void Perform(GameObject actor, TargetArgs target)
    //    {
    //        if (target.Object is Actor)
    //            throw new Exception();
    //        switch (target.Type)
    //        {
    //            case TargetType.Position:
    //                // check if hauling and drop at target position
    //                GameObject held = actor.GetComponent<HaulComponent>().Holding.Take();

    //                if (held == null)
    //                    return;
    //                //actor.Net.Spawn(held, target.FinalGlobal);
    //                held.Spawn(actor.Net.Map, target.FinalGlobal);
    //                break;

    //                // new: if inventoryable insert to inventory, if carryable carry
    //                // dont carry inventoriables (test)
    //            case TargetType.Entity:
    //                if(actor.InventoryContains(target.Object))
    //                {
    //                    PersonalInventoryComponent.HaulFromInventory(actor, target.Object, this.Amount);
    //                    break;
    //                }
    //                var containerGlobal = target.Global;
    //                var prevStackSize = target.Object.StackSize;

    //                if (!target.Object.IsHaulable)
    //                    throw new Exception();
    //                if (this.Amount > target.Object.StackSize)
    //                    throw new Exception();
    //                PersonalInventoryComponent.PickUpNewNew(actor, target.Object, this.Amount == - 1 ? target.Object.StackSize : this.Amount);

    //                // if target was in container, remove it from its contents
    //                if (this.Amount == prevStackSize && actor.Map.GetBlockEntity(containerGlobal) is BlockBin.BlockBinEntity container)
    //                    container.Remove(target.Object);

    //                break;



    //            case TargetType.Slot:
    //                var hauling = actor.GetComponent<HaulComponent>().GetSlot();//.Slot;

    //                if (hauling.HasValue)
    //                {
    //                    actor.Net.PostLocalEvent(target.Object, Message.Types.Insert, hauling);
    //                    break;
    //                }
    //                var obj = target.Slot.Object;
    //                if (obj.Physics.Size == ObjectSize.Inventoryable)
    //                    actor.Net.PostLocalEvent(actor, Message.Types.Insert, target.Slot);
    //                else if (obj.Physics.Size == ObjectSize.Haulable)
    //                {
    //                    if (actor.GetComponent<HaulComponent>().Carry(actor, target.Slot))
    //                        target.Slot.Clear();
    //                }
    //                break;


    //            default:
    //                break;
    //        }
    //    }

    //    public override object Clone()
    //    {
    //        return new InteractionHaul(this.Amount);
    //    }

    //    protected override void WriteExtra(System.IO.BinaryWriter w)
    //    {
    //        w.Write(this.Amount);
    //    }
    //    protected override void ReadExtra(System.IO.BinaryReader r)
    //    {
    //        this.Amount = r.ReadInt32();
    //    }
    //    protected override void AddSaveData(SaveTag tag)
    //    {
    //        tag.Add(this.Amount.Save("Amount"));
    //    }
    //    public override void LoadData(SaveTag tag)
    //    {
    //        tag.TryGetTagValue<int>("Amount", out this.Amount);
    //    }
    //}
}
