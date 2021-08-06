namespace Start_a_Town_.Components.Interactions
{
    class InteractionTilling : InteractionPerpetual
    {
        public InteractionTilling() : base("Till") { }

        public override void OnUpdate(Actor a, TargetArgs t)
        {
            a.Map.SetBlock(t.Global, BlockDefOf.Farmland, 0);
            this.Finish(a, t);
        }

        public override object Clone()
        {
            return new InteractionTilling();
        }
    }
}
