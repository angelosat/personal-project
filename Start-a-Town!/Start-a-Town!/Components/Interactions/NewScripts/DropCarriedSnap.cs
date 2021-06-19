using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.Animations;

namespace Start_a_Town_.Components.Interactions
{
    class DropCarriedSnap : Interaction
    {
        bool All;

        public DropCarriedSnap(bool all = false)
            : base(
            "Place down",
            //0,
            .4f
            )
        {
            this.All = all;
            //this.Animation = new Graphics.Animations.AnimationPlaceItem();
            //this.Animation = AnimationPlaceItem.PlaceItem();
        }

        static readonly TaskConditions conds = new TaskConditions(new AllCheck(
                new RangeCheck(t => t.FaceGlobal, RangeCheck.DefaultRange), new ScriptTaskCondition("IsCarrying", (a, t) => a.GetComponent<HaulComponent>().GetObject() != null, Message.Types.InteractionFailed)
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
            //this.Animation = AnimationPlaceItem.PlaceItem(a);
            this.Animation = new Animation(AnimationDef.TouchItem);

            var haul = a.GetComponent<HaulComponent>();
            //a.Body.FadeOutAnimation(haul.AnimationHaul, this.Seconds / 2f);// 1f);
            //a.Body.Start(this.Animation);
            haul.AnimationHaul.FadeOut(this.Seconds / 2f);// 1f);
            a.AddAnimation(this.Animation);
        }
        
        public override void Perform(GameObject actor, TargetArgs target)
        {
            //actor.GetComponent<HaulComponent>().Throw(Vector3.Zero, actor, this.All);
            var hauled = PersonalInventoryComponent.GetHauling(actor);
            var hauledObj = hauled.Object;
            hauled.Clear();
            
            //actor.Body.FadeOutAnimation(this.Animation);

            switch(target.Type)
            {
                case TargetType.Position:
                    // TODO: call a method in the block object to let it decide what to do with the dropped object?
                    // like, spawn it in the world, or receive it in its contents?
                    //hauledObj.SetGlobal(target.Global + target.Face);
                    //actor.Net.Spawn(hauledObj);
                    actor.Map.GetBlock(target.Global).OnDrop(actor, hauledObj, target);
                    break;

                case TargetType.Entity:
                    var o = target.Object;
                    if (o.StackSize + hauledObj.StackSize <= o.StackMax)
                    {
                        o.StackSize += hauledObj.StackSize;
                        hauledObj.Dispose();
                    }
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
            return this.Name + (this.All ? " All" : "");
        }

        public override object Clone()
        {
            return new DropCarriedSnap(this.All);
        }
    }
}
