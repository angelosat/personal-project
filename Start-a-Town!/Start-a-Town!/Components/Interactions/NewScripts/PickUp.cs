using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.Animations;

namespace Start_a_Town_.Components.Interactions
{
    class PickUp : Interaction
    {
        int Amount;
        public PickUp(int amount = -1)
            : base("PickUp", .4f)
        {
            //this.Animation = new Graphics.Animations.AnimationPlaceItem();
            //this.Animation = AnimationPlaceItem.PlaceItem();

            this.Amount = amount;
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
      
        public override string ToString()
        {
            return "PickUp " + (this.Amount == -1 ? " All" : " x" + this.Amount.ToString());
        }
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
            //this.Animation = AnimationPlaceItem.PlaceItem(a);
            //a.Body.CrossFade(this.Animation, false, 25);
            this.Animation = new Animation(AnimationDef.TouchItem);
            a.CrossFade(this.Animation, false, 25);
        }
        //protected override void Stop(GameObject actor)
        //{
        //    //actor.Body.FadeOutAnimation(this.Animation, 100 / Engine.TargetFps);
        //}
        public override void Perform(GameObject actor, TargetArgs target)
        {
            //PersonalInventoryComponent.PickUpNewNew(actor, target.Object, this.Amount == - 1 ? target.Object.StackSize : this.Amount);
            var inventory = actor.GetComponent<PersonalInventoryComponent>();
            inventory.Slots.InsertObject(target.Object as Entity);
        }

        public override object Clone()
        {
            return new PickUp(this.Amount);
        }

        protected override void WriteExtra(System.IO.BinaryWriter w)
        {
            w.Write(this.Amount);
        }
        protected override void ReadExtra(System.IO.BinaryReader r)
        {
            this.Amount = r.ReadInt32();
        }
    }
}
