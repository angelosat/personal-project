namespace Start_a_Town_.Components.Interactions
{
    class DropInventoryItem : Interaction
    {
        public DropInventoryItem()
            : base(
            "DropInventoryItem",
            0
            )
        {

        }
       
        public override void Perform(Actor actor, TargetArgs target)
        {
            actor.Inventory.Drop(target.Object);
        }

        public override object Clone()
        {
            return new DropInventoryItem();
        }
    }
}
