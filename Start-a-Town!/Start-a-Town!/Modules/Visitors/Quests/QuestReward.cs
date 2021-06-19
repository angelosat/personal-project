using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_
{
    public abstract class QuestReward
    {
        public QuestDef Parent;
        public QuestReward(QuestDef parent)
        {
            this.Parent = parent;
        }
        public abstract string Text { get; }
        public abstract string Label { get; }
        public abstract int Budget { get; }
        public abstract int Count { get; set; }

        internal abstract void Award(Actor actor);
        internal abstract bool CanAward();
    }
}
