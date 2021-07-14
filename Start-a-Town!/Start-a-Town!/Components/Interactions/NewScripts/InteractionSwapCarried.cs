using Start_a_Town_.Animations;

namespace Start_a_Town_
{
    class InteractionSwapCarried : Interaction
    {
        public InteractionSwapCarried()
             : base(
            "SwapCarried",
            .4f
            )
        {
        }
        public override object Clone()
        {
            return new InteractionSwapCarried();
        }
        public override void Start(Actor a, TargetArgs t)
        {
            this.Animation = new Animation(AnimationDef.TouchItem);
            a.CrossFade(this.Animation, false, 25);
        }
        public override void Perform(Actor a, TargetArgs t)
        {
            var item = t.Object as Entity;
            var global = item.Global;
            var actor = a as Actor;
            var prevCarried = actor.Carried;
            prevCarried.Slot.Clear();
            prevCarried.Spawn(a.Map, global);
            actor.Inventory.Haul(item);
        }
    }
}
