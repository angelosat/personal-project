﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.AI;
using Start_a_Town_.Components;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.AI.Behaviors
{
    class BehaviorCrouch : Behavior
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

            var map = parent.Map;
            var parentGlobal = parent.Global;
            if (parent.Mobile.Crouching)
            {
                var currentfootprintCorners = parent.GetBoundingBox(parentGlobal).GetCorners().Where(c => c.Z == parentGlobal.Z);
                if (currentfootprintCorners.All(c => !map.IsSolid(c.Above())))
                {
                    parent.CrouchToggle(false);
                    return BehaviorState.Success;
                }
            }
            else
            {
                var nextStep = parentGlobal + parent.Velocity * VelocityFactorForNextStep;
                var footprintCorners = parent.GetBoundingBox(nextStep).GetCorners().Where(c => c.Z == parentGlobal.Z);
                foreach (var corner in footprintCorners)
                {
                    var cell = parent.Map.GetCell(corner);
                    if (cell == null)
                        continue;
                    var isNextBlockSolid = cell.IsSolid();
                    var isAboveNextBlockSolid = parent.Map.IsSolid(corner.Above());
                    float nextBlockHeight = Block.GetBlockHeight(map, corner);
                    var heightDifference = (cell.Z + nextBlockHeight) - parentGlobal.Z;
                    if (!isNextBlockSolid && isAboveNextBlockSolid
                        //&& heightDifference > MaxClimbableHeight
                        )
                    {
                        //if (!parent.Mobile.Crouching)
                            parent.CrouchToggle(true);
                        return BehaviorState.Success;
                    }
                    else if (!isNextBlockSolid && !isAboveNextBlockSolid)
                    {

                        return BehaviorState.Success;
                    }
                }
            }
            return BehaviorState.Success;
        }
        //public override BehaviorState Execute(Actor parent, AIState state)
        //{
        //    var v = parent.Velocity;
        //    if (v.Z != 0)
        //        return BehaviorState.Success;
        //    if (parent.Acceleration == 0)
        //        return BehaviorState.Success;

        //    var parentGlobal = parent.Global;
        //    var nextStep = parentGlobal + parent.Velocity * VelocityFactorForNextStep;
        //    var footprintCorners = parent.GetBoundingBox(nextStep).GetCorners().Where(c => c.Z == parentGlobal.Z);
        //    foreach(var corner in footprintCorners)
        //    {
        //        var cell = parent.Map.GetCell(corner);
        //        if (cell == null)
        //            continue;
        //        var isNextBlockSolid = cell.IsSolid();
        //        var isAboveNextBlockSolid = parent.Map.IsSolid(corner.Above());
        //        float nextBlockHeight = Block.GetBlockHeight(parent.Map, corner);
        //        var heightDifference = (cell.Z + nextBlockHeight) - parentGlobal.Z;
        //        if (!isNextBlockSolid && isAboveNextBlockSolid 
        //            //&& heightDifference > MaxClimbableHeight
        //            )
        //        {
        //            //parent.Jump();
        //            if(!parent.Mobile.Crouching)
        //                parent.CrouchToggle(true);
        //            return BehaviorState.Success;
        //        }
        //        else if (!isNextBlockSolid && !isAboveNextBlockSolid)
        //        {
        //            if (parent.Mobile.Crouching)
        //                parent.CrouchToggle(false);
        //            return BehaviorState.Success;
        //        }
        //    }
        //    return BehaviorState.Success;
        //}
        public override object Clone()
        {
            throw new NotImplementedException();
        }
    }
}
