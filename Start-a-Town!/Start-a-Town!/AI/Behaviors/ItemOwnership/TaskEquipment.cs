using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.Components;

namespace Start_a_Town_.AI.Behaviors.ItemOwnership
{
    class TaskEquipment : AITask
    {
        public GearType Type;
        public TaskEquipment(Type behavType, GearType type)
        {
            this.BehaviorType = behavType;
            this.Type = type;
        }
    }
}
