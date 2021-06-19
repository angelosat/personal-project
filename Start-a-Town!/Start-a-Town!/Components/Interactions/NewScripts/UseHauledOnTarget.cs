using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components;
using Start_a_Town_.Animations;

namespace Start_a_Town_
{
    class UseHauledOnTarget : Interaction
    {
        int Amount;

        public UseHauledOnTarget()
             : base(
            "UseHauledOnTarget",
            //0,
            .4f
            )
        {
            this.Amount = -1;
            //this.Animation = new Graphics.Animations.AnimationPlaceItem();
            //this.Animation = AnimationPlaceItem.PlaceItem();

        }

        public UseHauledOnTarget(int amount = -1) // -1 means whole stack
            : base(
            "UseHauledOnTarget",
            //0,
            .4f
            )
        {
            if (amount == 0)
                throw new Exception();
            this.Amount = amount;

        }

        static readonly TaskConditions conds = new(new AllCheck(
            new RangeCheck(t => t.Global, RangeCheck.DefaultRange)
                )
            );

        public override TaskConditions Conditions
        {
            get 
	        { 
		         return conds;
	        }
        }

        public override bool AvailabilityCondition(GameObject actor, TargetArgs target)
        {
            //return GearComponent.GetSlot(actor, GearType.Hauling).Object != null;
            //return actor.GetComponent<HaulComponent>().Slot.Object != null;
            return actor.GetComponent<HaulComponent>().GetObject() != null;

        }
        public override void Start(GameObject a, TargetArgs t)
        {
            //this.Animation = AnimationPlaceItem.PlaceItem(a);
            this.Animation = new Animation(AnimationDef.TouchItem);
            //a.Body.CrossFade(this.Animation, false, 25);
            a.CrossFade(this.Animation, false, 25);
        }
        
        public override void Perform(GameObject actor, TargetArgs target)
        {
            //actor.GetComponent<HaulComponent>().Throw(Vector3.Zero, actor, this.All);
            var hauled = PersonalInventoryComponent.GetHauling(actor);
            var hauledObj = hauled.Object;
            // hauled.Clear();
            if (this.Amount > hauledObj.StackSize)
                throw new Exception();
            //actor.Body.FadeOutAnimation(this.Animation);
            //actor.Body.FadeOutAnimationAndRemove(this.Animation);
            this.Animation.FadeOutAndRemove();

            /// WTF IS THIS
            /// NO
            /// i placed this here when making the haultostockpile behavior so the actor 
            /// can combine item stacks without reserving the existing item
            /// so that i can be picked up by other actors in the meantime
            //var existing = actor.Map.GetObjects(target.Global);
            //var same = existing.FirstOrDefault(p => p.IDType == hauledObj.IDType);
            //if (same != null)
            //    target = new TargetArgs(same);
            /// NO

            switch (target.Type)
            {
                case TargetType.Position:
                    actor.Map.GetBlock(target.Global).OnDrop(actor, hauledObj, target, this.Amount == -1 ? hauledObj.StackSize : this.Amount);
                    actor.CurrentTask?.AddPlacedObject(hauledObj);
                    break;

                case TargetType.Entity:
                    var o = target.Object;
                    if (o.StackSize + this.Amount > o.StackMax)
                        throw new Exception();
                    var amount = (this.Amount == -1) ? hauledObj.StackSize : this.Amount;
                    if (!o.CanAbsorb(hauledObj, amount))
                        throw new Exception();
                    var transferAmount = Math.Min(o.StackAvailableSpace, amount);
                    o.StackSize += transferAmount;
                    hauledObj.StackSize -= transferAmount;
                    break;

                default:
                    break;
            }
            //if (hauledObj.IsDisposed)
            //    actor.Unreserve(hauledObj);
        }
        //public override void Perform(GameObject actor, TargetArgs target)
        //{
        //    //actor.GetComponent<HaulComponent>().Throw(Vector3.Zero, actor, this.All);
        //    var hauled = PersonalInventoryComponent.GetHauling(actor);
        //    var hauledObj = hauled.Object;
        //    hauled.Clear();
        //    hauledObj.SetGlobal(target.Global + target.Face);
        //    actor.Net.Spawn(hauledObj);
        //    //actor.Body.FadeOutAnimation(this.Animation);
        //}
        // TODO: make it so i have access to the carried item's stacksize, and include it in the name ( Throw 1 vs Throw 16 for example)
        public override string ToString()
        {
            return this.Name + (this.Amount != -1 ? " x" + this.Amount.ToString() : "All");
        }
        public override bool InRange(GameObject a, TargetArgs t)
        {
            var actorCoords = a.Global;//.Round();
            var actorBox = new BoundingBox(actorCoords - new Vector3(1, 1, 0), actorCoords + new Vector3(1, 1, a.Physics.Height));
            var targetBox = new BoundingBox(t.Global - Vector3.One, t.Global + Vector3.One);
            return actorBox.Intersects(targetBox);
        }
        public override object Clone()
        {
            return new UseHauledOnTarget(this.Amount);
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
