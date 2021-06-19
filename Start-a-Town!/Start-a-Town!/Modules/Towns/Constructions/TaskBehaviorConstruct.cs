using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.AI;
using Start_a_Town_.AI.Behaviors;
using Start_a_Town_.Components.Interactions;

namespace Start_a_Town_
{
    class TaskBehaviorConstruct : BehaviorPerformTask
    {
        TargetArgs Construction { get { return this.Task.GetTarget(ConstructionsID); } }
        public const TargetIndex ConstructionsID = TargetIndex.A;
        //public const TargetIndex ObstructingItemsID = TargetIndex.B;
        //public const TargetIndex ObstructingItemsDestinationsID = TargetIndex.C;

        protected override IEnumerable<Behavior> GetSteps()
        {
            var manager = this.Actor.Map.Town.ConstructionsManager;
            var actor = this.Actor;
            var map = actor.Map;
            //var startConstructing = new BehaviorGrabTool(); /*BehaviorHelper.ExtractNextTarget(ConstructionsID);*/
            //yield return BehaviorHelper.JumpIfTrue(startConstructing, () => !this.Task.GetTargetQueue(ObstructingItemsID).Any());
            //var jump = BehaviorHelper.ExtractNextTarget(ObstructingItemsID);
            //yield return jump;
            //yield return BehaviorHelper.ExtractNextTarget(ObstructingItemsDestinationsID);
            //yield return new BehaviorGetAtNewNew(ObstructingItemsID, 1);
            //yield return BehaviorHelper.StartCarrying(ObstructingItemsID);//.FailOn(deliverFail).FailOnUnavailableTarget(IngredientIndex);// new BehaviorInteractionNew(IngredientIndex, () => new Haul()).FailOnUnavailableTarget(IngredientIndex);
            //yield return new BehaviorGetAtNewNew(ObstructingItemsDestinationsID, 1);
            //yield return BehaviorHelper.PlaceCarried(ObstructingItemsDestinationsID);//.FailOn(deliverFail);
            //yield return BehaviorReserve.Release(ObstructingItemsID);
            //yield return BehaviorHelper.JumpIfMoreTargets(jump, ObstructingItemsID);

            bool endCond()
            {
                var constrTar = Construction;
                var result = constrTar.Type == TargetType.Null || !manager.IsBuildableCurrently(constrTar.Global);// !manager.IsDesignatedConstruction(constrTar.Global) || !manager.IsBuildable(constrTar.Global);
                //if(result)
                //    actor.NetNew.SyncReport($"{this} failed because construction at {Construction.Global} is invalid");
                return result;
            }
            this.FailOn(endCond);

            bool designationOccupied()
            {
                if (map.GetObjects(Construction.Global).Any(o => o != actor))
                {
                    //actor.NetNew.SyncReport($"{this} failed because construction at {Construction.Global} is obstructed");
                    return true;
                }
                return false;
            }
            yield return new BehaviorGrabTool();
            yield return new BehaviorGetAtNewNew(ConstructionsID).FailOn(designationOccupied);
            yield return new BehaviorInteractionNew(ConstructionsID, new Interactions.InteractionConstruct()).FailOn(designationOccupied);
            //yield return BehaviorReserve.Release(ConstructionsID);
            //yield return BehaviorHelper.JumpIfMoreTargets(startConstructing, ConstructionsID);

        }

        protected override bool InitExtraReservations()
        {
            var task = this.Task;
            var actor = this.Actor;
            var map = actor.Map;

            //var block = task.GetTarget(ConstructionsID).GetBlockEntity() as Blocks.BlockDesignation.BlockDesignationEntity;
            //foreach(var child in block.Children)
            //{
            //    actor.Reserve(new TargetArgs(map, child));
            //}
            var target = task.GetTarget(ConstructionsID);
            var entity = target.GetBlockEntity() as IConstructible;
            foreach (var child in entity.Children)
            {
                if(!actor.Reserve(new TargetArgs(map, child), 1))
                {
                    map.Net.Log.Write("failed to reserve child of multi-blocked construction");
                    return false;
                }
            }
            return true;
                //task.ReserveAll(actor, ObstructingItemsID) &&
                //task.ReserveAll(actor, ObstructingItemsDestinationsID);

        }

        public override BehaviorState Execute(Actor parent, AIState state)
        {
            var result = base.Execute(parent, state);
            if (result != BehaviorState.Running)
                "GAMW THN PANAGIA".ToConsole();
            return result;
        }
    }
}
