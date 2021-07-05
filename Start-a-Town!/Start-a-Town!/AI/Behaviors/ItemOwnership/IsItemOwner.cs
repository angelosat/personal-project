namespace Start_a_Town_.AI.Behaviors.ItemOwnership
{
    class IsItemOwner : BehaviorCondition
    {
        readonly GearType Type;
        public IsItemOwner(GearType geartype)
        {
            this.Type = geartype;
        }
        public override BehaviorState Execute(Actor parent, AIState state)
        {
            var item = GearComponent.GetSlot(parent, this.Type);
            var owns = OwnershipComponent.Owns(parent, item.Object);
            return owns ? BehaviorState.Success : BehaviorState.Fail;
        }
        public override object Clone()
        {
            return new IsItemOwner(this.Type);
        }
    }
}
