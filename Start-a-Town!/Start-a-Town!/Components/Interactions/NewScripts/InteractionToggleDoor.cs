using System;
namespace Start_a_Town_
{
    [Obsolete]
    public class InteractionToggleDoor : Interaction
    {
        public InteractionToggleDoor() : base("Open/close") { }
       
        internal override void InitAction(GameObject actor, TargetArgs target)
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
