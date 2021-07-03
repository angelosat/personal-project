using System;
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

        public override bool AvailabilityCondition(GameObject actor, TargetArgs target)
        {
            return actor.GetComponent<HaulComponent>().GetObject() != null;

        }
        public override void Start(GameObject a, TargetArgs t)
        {
            this.Animation = new Animation(AnimationDef.TouchItem);
            var haul = a.GetComponent<HaulComponent>();
            haul.AnimationHaul.FadeOut(this.Seconds / 2f);
            a.AddAnimation(this.Animation);
        }
        
        public override void Perform(GameObject actor, TargetArgs target)
        {
            var hauled = PersonalInventoryComponent.GetHauling(actor);
            var hauledObj = hauled.Object;
            hauled.Clear();
           
            switch(target.Type)
            {
                case TargetType.Position:
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
