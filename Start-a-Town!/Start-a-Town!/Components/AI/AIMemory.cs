namespace Start_a_Town_.AI.Behaviors
{
    class AIMemory : Behavior
    {
        float Timer { get; set; }
        float Period { get; set; }
        public AIMemory()
        {
            this.Timer = 0;
            this.Period = Engine.TicksPerSecond;
        }
        public override BehaviorState Execute(Actor parent, AIState state)
        {
            if (this.Timer < Period)
            {
                this.Timer++;
                // return fail so we don't block parent selector
                return BehaviorState.Fail;//.Running;
            }
            this.Timer = 0;
            state.Knowledge.Update();
            // return fail so we don't block parent selector
            return BehaviorState.Fail;
        }

        public override object Clone()
        {
            return new AIMemory();
        }
    }
}
