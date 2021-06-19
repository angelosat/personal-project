using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.Blocks;

namespace Start_a_Town_.AI.Behaviors
{
    class BuildingHasMaterials : BehaviorCondition
    {
        string Target;
        public BuildingHasMaterials(string target)
        {
            this.Target = target;
        }
        public override bool Evaluate(GameObject agent, AIState state)
        {
            var target = state.Blackboard[this.Target] as TargetArgs;
            var blockentity = agent.Map.GetBlockEntity(target.Global) as BlockDesignation.BlockDesignationEntity;
            return blockentity.MaterialsPresent();
        }
    }
}
