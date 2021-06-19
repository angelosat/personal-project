using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.Components.Skills;
using Start_a_Town_.Components.AI;
using Start_a_Town_.UI;
using Start_a_Town_.AI;

namespace Start_a_Town_
{
    public class Exists : ScriptTaskCondition
    {
        public Exists()
            : base("Exists")
        {

        }

        public override bool Condition(GameObject actor, TargetArgs target)
        {
            return target.Object.IsSpawned;
        }
        //public override bool AITrySolve(GameObject agent, TargetArgs target, Components.AI.AIState state, out AIInstruction instruction)
        //{
        //    instruction = null;
        //    return false;
        //}

    }
}
