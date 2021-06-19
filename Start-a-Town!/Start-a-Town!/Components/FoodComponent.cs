using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_.Components
{
    class FoodComponent : EntityComponent
    {
        public override string ComponentName => "FoodComponent";

        public override object Clone()
        {
            return new FoodComponent();
        }
    }
}
