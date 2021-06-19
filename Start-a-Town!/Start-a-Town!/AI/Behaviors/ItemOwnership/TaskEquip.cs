using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_.AI.Behaviors.ItemOwnership
{
    class TaskEquip : AITask
    {
        public GameObject Item;

        public TaskEquip(GameObject item)
        {
            // TODO: Complete member initialization
            this.Item = item;
            //this.Behavior = new BehaviorEquipItem(this);
        }
        
        //public override Behavior GetBehavior(GameObject actor)
        //{
        //    return new BehaviorGetItem(this);
        //}
    }
}
