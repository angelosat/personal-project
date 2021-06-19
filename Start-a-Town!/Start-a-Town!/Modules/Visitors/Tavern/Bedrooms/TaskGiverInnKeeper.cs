using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_
{
    class TaskGiverInnKeeper : TaskGiver
    {
        protected override AITask TryAssignTask(Actor actor)
        {
            var workplace = actor.Workplace as Tavern;
            var customers = workplace.Customers;
            foreach (var customer in customers.ToArray())
            {
                if (customer.Bedroom is not null && 
                    !customer.Customer.Ownership.Owns(customer.Bedroom))
                {
                    return new AITask(typeof(TaskBehaviorInnKeeper), customer.Customer);// { CustomerProps = customer };
                }
            }
            return null;
        }
    }
}
