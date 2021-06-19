using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_.AI.Behaviors
{
    class TaskNotCancelled : BehaviorCondition
    {
        AITask Task;
        string TaskName;
        public TaskNotCancelled(AITask task)
        {
            this.Task = task;
        }
        public TaskNotCancelled(string taskName)
        {
            this.TaskName = taskName;
        }
        public override bool Evaluate(GameObject agent, AIState state)
        {
            var task = this.Task ?? (state.Blackboard[this.TaskName] as AITask);
            return !task.IsCancelled;
            //return this.Task != null ? !this.Task.IsCancelled : !(state.Blackboard[this.TaskName] as AITask).IsCancelled;
        }
    }
}
