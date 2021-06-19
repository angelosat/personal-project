using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_.AI
{
    public class AIGoalState
    {
        public string Name { get; private set; }
        Func<GameObject, TargetArgs, bool> Condition;
        public AIGoalState(string name, Func<GameObject, TargetArgs, bool> condition)
        {
            this.Name = name;
            this.Condition = condition;
        }
        public bool FindPlan(GameObject actor, out AIInstruction instruction)
        {
            instruction = null;
            return false;
        }
        public bool IsMet(GameObject a, TargetArgs t)
        {
            return this.Condition(a, t);
        }
    }
}
