﻿using System;

namespace Start_a_Town_.AI.Behaviors
{
    class AIWait : Behavior
    {
        public override string Name
        {
            get
            {
                return "Idle";
            }
        }
        int TimerNew = 0;
        public AIWait() : this(new TimeSpan(0, 0, 1)) { }
        int BaseWaitTime = 5;
        public AIWait(TimeSpan timer)
        {
        }
        public override BehaviorState Execute(Actor parent, AIState state)
        {
            this.TimerNew--;
            if (this.TimerNew <= 0)
            {
                var composure = parent.GetTrait(TraitDefOf.Composure).Normalized;
                this.TimerNew = (int)((BaseWaitTime + (.5f * BaseWaitTime * composure)) * Engine.TicksPerSecond);
                return BehaviorState.Success;
            }
            return BehaviorState.Running;
        }
        public override object Clone()
        {
            return new AIWait();
        }
        public override void Write(System.IO.BinaryWriter w)
        {
            base.Write(w);
            w.Write(this.TimerNew);
        }
        public override void Read(System.IO.BinaryReader r)
        {
            base.Read(r);
            this.TimerNew = r.ReadInt32();
        }
        protected override void AddSaveData(SaveTag tag)
        {
            base.AddSaveData(tag);
            tag.Add(this.TimerNew.Save("Timer"));
        }
        internal override void Load(SaveTag tag)
        {
            base.Load(tag);
            tag.TryGetTagValue<int>("Timer", out this.TimerNew);
        }
    }
}
