namespace Start_a_Town_
{
    public class InteractionToggleDoor : Interaction
    {
        public InteractionToggleDoor() : base("Open/close") { }
       
        internal override void InitAction(Actor actor, TargetArgs target)
        {
            base.InitAction(actor, target);
            BlockDoor.Toggle(actor.Map, target.Global);
        }
        public override object Clone()
        {
            return new InteractionToggleDoor();
        }
    }
}
