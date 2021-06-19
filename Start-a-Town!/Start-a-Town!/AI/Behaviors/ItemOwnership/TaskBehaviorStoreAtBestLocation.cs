using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_.AI.Behaviors.ItemOwnership
{
    class TaskBehaviorStoreAtBestLocation : BehaviorPerformTask
    {
        protected override IEnumerable<Behavior> GetSteps()
        {
            throw new NotImplementedException();
        }
        static public void DetermineBestLocation(GameObject actor, GameObject item)
        {
            var possesions = actor.GetPossesions();
            if(possesions.Contains(item))
            {
                //store in inventory
            }
            else
            {
                //actor.Map.Town.StockpileManager.get
                //find stockpile
            }
            //else
            //{
            //    //drop on ground
            //}
        }
    }
}
