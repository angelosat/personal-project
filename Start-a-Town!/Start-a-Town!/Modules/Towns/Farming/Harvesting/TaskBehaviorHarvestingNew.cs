using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.AI;
using Start_a_Town_.AI.Behaviors;
using Start_a_Town_.Components;

namespace Start_a_Town_
{
    class TaskBehaviorHarvestingNew : BehaviorPerformTask
    {
        public const TargetIndex PlantIndex = TargetIndex.A;
        protected override IEnumerable<Behavior> GetSteps()
        {
            this.FailOn(() =>
            {
                var target = this.Task.GetTarget(PlantIndex);
                var plant = target.Object as Plant;
                if (plant == null)
                    return true;
                if (!plant.IsSpawned)
                    return true;
                if (!plant.IsHarvestable)
                    return true;
                return false;
            });
            this.FailOnForbidden(PlantIndex);
            //this.FailOn(() => !IsValidTarget());
            yield return new BehaviorGetAtNewNew(PlantIndex);//.While(() => this.IsValidTarget(this.Task.Target.Object));
            yield return new BehaviorInteractionNew(PlantIndex, () => new InteractionHarvest());//.While(() => this.IsValidTarget(this.Task.Target.Object));
        }
        protected override bool InitExtraReservations()
        {
            return this.Actor.Reserve(this.Task.GetTarget(PlantIndex));
        }
        //public override bool HasFailedOrEnded()
        //{
        //    return false;
        //    var plant = this.Task.GetTarget(PlantIndex).Object as Plant;
        //    var isvalid =
        //        (plant != null && plant.IsSpawned) &&
        //        //PlantComponent.IsGrown(plant) &&
        //        plant.IsHarvestable &&
        //        (this.Actor.Map.Town.ChoppingManager.IsForagingTask(plant) || this.Actor.Map.Town.FarmingManager.IsHarvestTask(plant));
        //    return !isvalid;
        //}
    }
}
