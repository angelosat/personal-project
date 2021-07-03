using System.Collections.Generic;

namespace Start_a_Town_.AI.Behaviors
{
    class BehaviorIdle : BehaviorSequence
    {
        public BehaviorIdle()
        {
            this.Children = new List<Behavior>()
            {
                new AIWait(),
                new AIWander()
            };
        }
        public override object Clone()
        {
            return new BehaviorIdle();
        }
        internal override void Load(SaveTag tag)
        {
            base.Load(tag);
            this.Children.Count.ToConsole();
        }
    }
}
