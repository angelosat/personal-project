using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_
{
    class TaskBehaviorTavernWorkerTakeOrder : BehaviorPerformTask
    {
        protected override IEnumerable<Behavior> GetSteps()
        {
            var actor = this.Actor;
            var task = this.Task;
            var customerIndex = TargetIndex.A;
            //var customerProps = task.CustomerProps as Tavern.CustomerTavern;
            var customer = task.GetTarget(customerIndex);
            var shop = actor.Workplace as Tavern;// actor.Town.GetShop(task.ShopID) as Tavern;
            var customerProps = shop.GetCustomer(customer) as CustomerTavern;

            // TODO wait until customer is seated
            yield return new BehaviorWait(() => customerProps.IsSeated);
            yield return BehaviorHelper.MoveTo(customerIndex);
            yield return new BehaviorCustom() { InitAction = () => customerProps.OrderTakenBy = actor };
        }
    }
}
