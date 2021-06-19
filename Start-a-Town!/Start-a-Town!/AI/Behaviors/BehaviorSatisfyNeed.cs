using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.Components;
using Start_a_Town_.Net;

namespace Start_a_Town_.AI.Behaviors
{
    class BehaviorSatisfyNeed : Behavior
    {
        int Timer, TimerMax = Engine.TicksPerSecond;
        Behavior Child;
        public override string Name
        {
            get
            {
                return "BehaviorSatisfyNeed";
            }
        }
        public override BehaviorState Execute(Actor parent, AIState state)
        {
            if (this.Child != null)
            {
                var result = this.Child.Execute(parent, state);
                switch (result)
                {
                    case BehaviorState.Running:
                        Start_a_Town_.Modules.AI.Net.PacketTaskUpdate.Send(parent.Net as Server, parent.RefID, this.Child.Name);
                        return BehaviorState.Success;

                    case BehaviorState.Success:
                    case BehaviorState.Fail:
                        var bhav = this.FindBehav(parent, state);
                        if (bhav != null)
                        {
                            this.Child = bhav;
                            return BehaviorState.Success;
                        }
                        else
                        this.Child = null;
                        break;

                    default:
                        break;
                }
            }
            else
            {
                if (this.Timer < this.TimerMax)
                {
                    this.Timer++;
                }
                else
                {
                    this.Timer = 0;
                    var bhav = this.FindBehav(parent, state);
                    if (bhav != null)
                    {
                        this.Child = bhav;
                        return BehaviorState.Success;
                    }
                    //var needs = parent.GetComponent<NeedsComponent>().NeedsHierarchy;
                    //foreach (var category in needs.Inner.Values)
                    //{
                    //    foreach (var need in category.Values.OrderBy(n => n.Value))// <= n.Max / 2f))
                    //    {
                    //        //if (need.Value > need.Threshold) //threshold
                    //        //continue;
                    //        var needBehav = need.GetBehavior(parent);
                    //        if (needBehav == null)
                    //            continue;
                    //        var result = needBehav.Execute(parent, state);
                    //        switch(result)
                    //        {
                    //            case BehaviorState.Running:
                    //                this.Child = needBehav;
                    //                return BehaviorState.Success;
                    //                break;

                    //            default:
                    //                break;
                    //        }
                    //    }
                    //}
                }
            }
            Start_a_Town_.Modules.AI.Net.PacketTaskUpdate.Send(parent.Net as Server, parent.RefID, "Idle");

            return BehaviorState.Fail;// Success;
        }
        Behavior FindBehav(Actor parent, AIState state)
        {
            throw new Exception();
            //var needs = parent.GetComponent<NeedsComponent>().NeedsHierarchy;
            //foreach (var category in needs.Inner.Values)
            //{
            //    foreach (var need in category.Values.OrderBy(n => n.Value))// <= n.Max / 2f))
            //    {
            //        //if (need.Value > need.Threshold) //threshold
            //        //continue;
            //        var needBehav = need.GetBehavior(parent);
            //        if (needBehav == null)
            //            continue;
            //        var result = needBehav.Execute(parent, state);
            //        switch (result)
            //        {
            //            case BehaviorState.Running:
            //                return needBehav;

            //            default:
            //                break;
            //        }
            //    }
            //}
            //return null;
        }
        //public override BehaviorState Execute(Entity parent, AIState state)
        //{
        //    if (this.Child != null)
        //    {
        //        var result = this.Child.Execute(parent, state);
        //        switch (result)
        //        {
        //            case BehaviorState.Running:
        //                Start_a_Town_.Modules.AI.Net.PacketTaskUpdate.Send(parent.Net as Server, parent.InstanceID, this.Child.Name);
        //                return BehaviorState.Success;
        //                break;

        //            case BehaviorState.Success:
        //            case BehaviorState.Fail:
        //                this.Child = null;
        //                Start_a_Town_.Modules.AI.Net.PacketTaskUpdate.Send(parent.Net as Server, parent.InstanceID, "Idle");
        //                break;

        //            default:
        //                break;
        //        }
        //    }
        //    else
        //    {
        //        if (this.Timer < this.TimerMax)
        //        {
        //            this.Timer++;
        //        }
        //        else
        //        {
        //            this.Timer = 0;
        //            var needs = parent.GetComponent<NeedsComponent>().NeedsHierarchy;
        //            foreach (var category in needs.Inner.Values)
        //            {
        //                foreach (var need in category.Values.OrderBy(n => n.Value))// <= n.Max / 2f))
        //                {
        //                    //if (need.Value > need.Threshold) //threshold
        //                        //continue;
        //                    this.Child = need.GetBehavior(parent);
        //                    if (this.Child != null)
        //                        return BehaviorState.Success;
        //                }
        //            }
        //        }
        //    }
        //    return BehaviorState.Fail;// Success;
        //}
        public override object Clone()
        {
            return new BehaviorSatisfyNeed();
        }
    }
}
