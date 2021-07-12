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
       
        static readonly TaskConditions conds = new TaskConditions(new AllCheck(
                new RangeCheck(t => t.Global, Interaction.DefaultRange),
                new AnyCheck(
                    new TargetTypeCheck(TargetType.Position),
                    new TargetTypeCheck(TargetType.Entity)))
            );
        public override TaskConditions Conditions
        {
            get
            {
                return conds;
            }
        }
        public override void Perform(GameObject actor, TargetArgs target)
        {
            actor.Inventory.DropInventoryItem(target.Object);
        }

        public override object Clone()
        {
            return new DropInventoryItem();
        }
    }
}
