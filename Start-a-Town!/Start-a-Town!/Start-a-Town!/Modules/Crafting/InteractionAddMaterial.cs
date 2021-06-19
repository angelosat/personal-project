using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.Components.Interactions;
using Start_a_Town_.Components;

namespace Start_a_Town_.Crafting
{
    class InteractionAddMaterial : Interaction
    {
        //Entity Entity;
        public InteractionAddMaterial()//Entity entity)
            : base(
            "Add Material",
            .4f)
        {
            //this.Name = "Add Material";
            //this.Length = 1;
            this.Animation = new Graphics.Animations.AnimationPlaceItem();
            //this.Entity = entity;
        }

        static readonly TaskConditions conds = new TaskConditions(new AllCheck(
                //new RangeCheck(1)// RangeCheck.Default
                //,
                RangeCheck.Sqrt2,
                new IsHauling()//this.Entity.IsMaterialValid)
                ));
        public override TaskConditions Conditions
        {
            get
            {
                return conds;
            }
        }

        public override void Start(GameObject a, TargetArgs t)
        {
            base.Start(a, t);
            var haul = a.GetComponent<HaulComponent>();
            a.Body.FadeOutAnimation(haul.AnimationHaul, this.Seconds / 2f);// 1f);
            a.Body.Start(this.Animation);
        }

        public override void Perform(GameObject actor, TargetArgs target)
        {
            var block = actor.Map.GetBlock(target.Global);
            var hauled = PersonalInventoryComponent.GetHauling(actor);
            var hauledObj = hauled.Object;
            hauled.Clear();
            actor.Map.GetBlock(target.Global).OnDrop(actor, hauledObj, target);
        }

        public override object Clone()
        {
            return new InteractionAddMaterial();//this.Entity);
        }
    }

}
