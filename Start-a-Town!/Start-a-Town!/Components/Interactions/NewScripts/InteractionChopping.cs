namespace Start_a_Town_
{
    public class InteractionChopping : Interaction
    {
        public InteractionChopping()
            : base(
                "Chop",
                1
                )
        {
            this.Verb = "Chopping";
            this.Skill = ToolAbilityDef.Chopping;
        }
       
        public override bool AvailabilityCondition(GameObject actor, TargetArgs target)
        {
            return this.Conditions.GetFailedCondition(actor, target) == null;
        }

        public override void Perform(GameObject a, TargetArgs t)
        {
            var cell = a.Map.GetCell(t.Global);
            var block = cell.Block;
            block.Break(a, t.Global);
        }

        public override object Clone()
        {
            return new InteractionChopping();
        }
    }
}
