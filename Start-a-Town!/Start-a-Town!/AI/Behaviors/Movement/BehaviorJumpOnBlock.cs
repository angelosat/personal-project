using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.AI;
using Start_a_Town_.Components;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.AI.Behaviors
{
    class BehaviorJumpOnBlock : Behavior
    {
        const float MaxClimbableHeight = .5f;
        const float VelocityFactorForNextStep = 4;
        public override BehaviorState Execute(Actor parent, AIState state)
        {
            var v = parent.Velocity;
            if (v.Z != 0)
                return BehaviorState.Success;
            if (parent.Acceleration == 0)
                return BehaviorState.Success;

            var parentGlobal = parent.Global;
            var nextStep = parentGlobal + parent.Velocity * VelocityFactorForNextStep;
            var footprintCorners = parent.GetBoundingBox(nextStep).GetCorners().Where(c => c.Z == parentGlobal.Z);
            foreach(var corner in footprintCorners)
            {
                var cell = parent.Map.GetCell(corner);
                if (cell == null)
                    continue;
                var isNextBlockSolid = cell.IsSolid();
                var isAboveNextBlockSolid = parent.Map.IsSolid(corner.Above());
                float nextBlockHeight = Block.GetBlockHeight(parent.Map, corner);
                var heightDifference = (cell.Z + nextBlockHeight) - parentGlobal.Z;
                if (isNextBlockSolid && !isAboveNextBlockSolid &&
                    heightDifference > MaxClimbableHeight)
                    //nextBlockHeight > MaxClimbableHeight)
                {
                    parent.Jump();
                    return BehaviorState.Success;
                }
            }
            return BehaviorState.Success;

            //var nextStep = parent.Global + parent.Direction * .5f; //parent.Velocity;// +parent.Direction * .5f;
            //var isNextBlockSolid = parent.Map.IsSolid(nextStep);
            //float nextBlockHeight = Block.GetBlockHeight(parent.Map, nextStep);

            //var isAboveNextBlockSolid = parent.Map.IsSolid(nextStep + Vector3.UnitZ);

            //if (isNextBlockSolid && !isAboveNextBlockSolid && nextBlockHeight > .5f)
            //    parent.Jump();
            //return BehaviorState.Success;
        }
        public override object Clone()
        {
            throw new NotImplementedException();
        }
    }
}
