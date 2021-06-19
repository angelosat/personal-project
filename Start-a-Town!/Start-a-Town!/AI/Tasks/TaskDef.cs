using Start_a_Town_.AI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_
{
    public sealed class TaskDef : Def
    {
        public Type BehaviorClass;
        public string Format;
        public TargetIndex PrimaryTargetIndex;
        public Func<AITask, TargetArgs> GetPrimaryTarget;
        public TaskDef(string name, Type bhavClass):base(name)
        {
            this.BehaviorClass = bhavClass;
        }
        public string GetForceText(AITask task)
        {
            //return string.Format(this.Format, task.GetTarget(this.PrimaryTargetIndex).Label);
            return string.Format(this.Format, this.GetPrimaryTarget(task).Label);
        }
    }
}
