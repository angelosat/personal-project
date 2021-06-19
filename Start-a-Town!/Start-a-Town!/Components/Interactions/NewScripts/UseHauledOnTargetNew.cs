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
    class UseHauledOnTargetNew : Interaction
    {
        int Amount;

        public UseHauledOnTargetNew()
             : base(
            "UseHauledOnTarget",
            //0,
            .4f
            )
        {
            this.Amount = -1;
            this.Animation = new Animation(AnimationDef.TouchItem);
        }

        public UseHauledOnTargetNew(int amount = -1) // -1 means whole stack
            : base(
            "UseHauledOnTarget",
            //0,
            .4f
            )
        {
            if (amount == 0)
                throw new Exception();
            this.Amount = amount;
            this.Animation = new Animation(AnimationDef.TouchItem);
        }

        static readonly TaskConditions conds = new TaskConditions(new AllCheck(
            //new RangeCheck(t => t.FaceGlobal, RangeCheck.DefaultRange)//InteractionOld.DefaultRange)//
            //new RangeCheck(t => t.Global + Vector3.UnitZ, RangeCheck.DefaultRange)//InteractionOld.DefaultRange)//
            new RangeCheck(t => t.Global, RangeCheck.DefaultRange)//InteractionOld.DefaultRange)//

                //,
                //new ScriptTaskCondition("IsCarrying", (a, t) => a.GetComponent<HaulComponent>().GetObject() != null, Message.Types.InteractionFailed)
                )
            );

        public override TaskConditions Conditions
        {
            get 
	        { 
		         return conds;
	        }
        }
        // TODO: cancel state ishauling

        public override bool AvailabilityCondition(GameObject actor, TargetArgs target)
        {
            //return GearComponent.GetSlot(actor, GearType.Hauling).Object != null;
            //return actor.GetComponent<HaulComponent>().Slot.Object != null;
            return actor.GetComponent<HaulComponent>().GetObject() != null;

        }
        public override void Start(GameObject a, TargetArgs t)
        {
            //this.Animation = new Animation(AnimationDef.TouchItem);
            a.CrossFade(this.Animation, false, 25);
        }
        
        public override void Perform(GameObject actor, TargetArgs target)
        {
            //actor.GetComponent<HaulComponent>().Throw(Vector3.Zero, actor, this.All);
            var hauled = PersonalInventoryComponent.GetHauling(actor);
            var hauledObj = hauled.Object;
            if(hauledObj == null)
            {
                actor.Net.Log.Write(actor.Name + " tried to drop hauled object but was wan't hauling anything");
                this.State = States.Failed;
                return;
            }
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


                    // TODO: call a method in the block object to let it decide what to do with the dropped object?
                    // like, spawn it in the world, or receive it in its contents?
                    //hauledObj.SetGlobal(target.Global + target.Face);
                    //actor.Net.Spawn(hauledObj);
                   
                    actor.Map.GetBlock(target.Global).OnDrop(actor, hauledObj, target, this.Amount == -1 ? hauledObj.StackSize : this.Amount);
                    actor.CurrentTask?.AddPlacedObject(hauledObj);
                    //if (hauled.Object != null)
                    //    if (hauled.Object.StackSize == 0)
                    //        hauled.Clear();
                    //if (hauled.Object != null)
                    //    if (this.Amount == hauled.Object.StackSize)
                    //        hauled.Clear(); // VERY UGLY HACKY 
                    // NOOOO!!!! don't change stacksize or clear haulslot here! let the block change it on its ondrop method. 


                    // TODO: I DONT SEND INTERACTION ARGUEMENTS TO CLIENTS!!!!
                    //if(this.Amount == hauledObj.StackSize || this.Amount == -1)
                    //    hauled.Clear();
                    break;

                case TargetType.Entity:
                    var o = target.Object;
                   
                    var amount = (this.Amount == -1) ? hauledObj.StackSize : this.Amount;
                    var transferAmount = Math.Min(o.StackAvailableSpace, amount); // do i want to check this here? 
                    if (o.StackSize + transferAmount > o.StackMax)
                        throw new Exception();
                    if (!o.CanAbsorb(hauledObj, transferAmount))
                        throw new Exception();

                    //o.StackSize += amount;
                    //hauledObj.StackSize -= amount;

                    //var transferAmount = Math.Min(o.StackAvailableSpace, amount);
                    o.StackSize += transferAmount;
                    hauledObj.StackSize -= transferAmount;

                    break;

                default:
                    break;
            }
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
            return new UseHauledOnTargetNew(this.Amount);
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
