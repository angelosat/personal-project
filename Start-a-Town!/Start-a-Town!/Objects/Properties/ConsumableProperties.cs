using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_
{
    public class ConsumableProperties
    {

        public FoodClass[] FoodClasses;
        internal Func<Entity, Entity> Byproduct;
    }
}
