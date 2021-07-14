namespace Start_a_Town_
{
    class InteractionFlipSwitch : Interaction
    {
        public override void Perform(Actor a, TargetArgs t)
        {
            var e = a.Map.GetBlockEntity(t.Global);
            e.GetComp<BlockEntityCompSwitchable>().Toggle(a, t);
            this.Finish(a, t);
        }
        public override object Clone()
        {
            return new InteractionFlipSwitch();
        }
    }
}
