using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.AI.Behaviors
{
    class BehaviorUnstuck : Behavior
    {
        //bool Correcting;
        int Timer;
        readonly int TimerMax = Engine.TicksPerSecond;
        Vector3 LastPosition;
        public override BehaviorState Execute(Actor parent, AIState state)
        {
            var a = parent.Acceleration;
            //var v = parent.Velocity;
            if (this.Timer == this.TimerMax)
            {
                var distanceVector = parent.Global.Round() - parent.Global;
                distanceVector.Z = 0;
                var l = distanceVector.Length();
                if (l < .1f)
                {
                    // arrived
                    this.Timer = 0;
                    return BehaviorState.Success;
                }
                var dir = distanceVector;
                dir.Normalize();
                parent.Direction = dir;
                return BehaviorState.Running;
            }
            else if (a > 0 && parent.Global == this.LastPosition)
                    this.Timer++;
            this.LastPosition = parent.Global;
            return BehaviorState.Success;

            //if (a > 0 && v == Vector3.Zero)
            //{
            //    var distanceVector = parent.Global.Round() - parent.Global;
            //    //var l = distanceVector.Length();
            //    //if (l <= .1f)
            //    //{
            //    //    this.Correcting = false;
            //    //    return BehaviorState.Success;
            //    //}
            //    //if (!this.Correcting)
            //    //{
            //    var dir = distanceVector;
            //    dir.Normalize();
            //    parent.Direction = dir;
            //    this.Correcting = true;
            //    return BehaviorState.Running;
            //    //}
            //}
            //if (!this.Correcting)
            //    return BehaviorState.Success;
            //var d = Vector3.Distance(parent.Global.Round(), parent.Global);
            //if (d < .1f)
            //{
            //    this.Correcting = false;
            //    return BehaviorState.Success;
            //}
            //return BehaviorState.Running;
        }

        //public override BehaviorState Execute(Entity parent, AIState state)
        //{
        //    var a = parent.Acceleration;
        //    var v = parent.Velocity;
        //    if (a > 0 && v == Vector3.Zero)
        //    {
        //        var distanceVector = parent.Global.Round() - parent.Global;
        //        //var l = distanceVector.Length();
        //        //if (l <= .1f)
        //        //{
        //        //    this.Correcting = false;
        //        //    return BehaviorState.Success;
        //        //}
        //        //if (!this.Correcting)
        //        //{
        //        var dir = distanceVector;
        //        dir.Normalize();
        //        parent.Direction = dir;
        //        this.Correcting = true;
        //        return BehaviorState.Running;
        //        //}
        //    }
        //    if (!this.Correcting)
        //        return BehaviorState.Success;
        //    var d = Vector3.Distance(parent.Global.Round(), parent.Global);
        //    if (d < .1f)
        //    {
        //        this.Correcting = false;
        //        return BehaviorState.Success;
        //    }
        //    return BehaviorState.Running;
        //}
        public override object Clone()
        {
            throw new NotImplementedException();
        }
    }
}
