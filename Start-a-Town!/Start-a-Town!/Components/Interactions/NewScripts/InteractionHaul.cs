using System;
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
            a.CrossFade(this.Animation, false, 25);
        }
        public override void OnUpdate(GameObject actor, TargetArgs target)
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
                    if (this.Amount == prevStackSize && actor.Map.GetBlockEntity(containerGlobal) is BlockStorage.BlockStorageEntity container)
                        container.Remove(target.Object);
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
}
