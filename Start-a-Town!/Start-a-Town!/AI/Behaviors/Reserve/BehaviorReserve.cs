using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_.AI.Behaviors
{
    static class BehaviorReserve// : BehaviorCustom
    {
        //static public Behavior Reserve(TargetArgs target, int amount = 1)
        //{
        //    var bhav = new BehaviorCustom();
        //    bhav.InitAction = () =>
        //    {
        //        if (!bhav.Actor.Reserve(bhav.Actor.CurrentTask, target, amount))
        //            throw new Exception();
        //    };
        //    return bhav;
        //}
        static public Behavior ReserveCarried()
        {
            var bhav = new BehaviorCustom();
            bhav.InitAction = () =>
            {
                bhav.Actor.Reserve(bhav.Actor.Carried);
            };
            return bhav;
        }
        static public Behavior Reserve(TargetIndex targetInd)
        {
            var bhav = new BehaviorCustom();
            bhav.InitAction = () =>
            {
                if (!bhav.Actor.Reserve(bhav.Actor.CurrentTask, bhav.Actor.CurrentTask.GetTarget(targetInd), -1))
                    throw new Exception();
            };
            return bhav;
        }
        static public Behavior Reserve(TargetIndex targetInd, TargetIndex amountInd)
        {
            var bhav = new BehaviorCustom();
            bhav.InitAction = () =>
            {
                if (!bhav.Actor.Reserve(bhav.Actor.CurrentTask, bhav.Actor.CurrentTask.GetTarget(targetInd), bhav.Actor.CurrentTask.GetAmount(amountInd)))
                    throw new Exception();
            };
            return bhav;
        }
        
        static public Behavior Release(TargetIndex targetInd)//, int amount = 1)
        {
            var bhav = new BehaviorCustom();
            bhav.InitAction = () =>
            {
                if (!bhav.Actor.Town.ReservationManager.ReservedBy(bhav.Actor.CurrentTask.GetTarget(targetInd), bhav.Actor, bhav.Actor.CurrentTask))
                    throw new Exception();
                bhav.Actor.Town.ReservationManager.Unreserve(bhav.Actor, bhav.Actor.CurrentTask);
            };
            return bhav;
        }

    }
}
