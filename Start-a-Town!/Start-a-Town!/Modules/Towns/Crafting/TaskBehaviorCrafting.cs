using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.AI;
using Start_a_Town_.AI.Behaviors;
using Start_a_Town_.Components.Interactions;
using Start_a_Town_.Crafting;
using Start_a_Town_.PathFinding;
using Start_a_Town_.Towns.Crafting;

namespace Start_a_Town_
{
    class TaskBehaviorCrafting : BehaviorPerformTask
    {
        TargetArgs Ingredient { get { return this.Task.GetTarget(IngredientIndex); } }
        TargetArgs Workstation { get { return this.Task.GetTarget(WorkstationIndex); } }
        TargetArgs AuxiliaryPosition { get { return this.Task.GetTarget(AuxiliaryIndex); } }
        public static TargetIndex AuxiliaryIndex = TargetIndex.C;
        public static TargetIndex IngredientIndex = TargetIndex.B;
        public static TargetIndex WorkstationIndex = TargetIndex.A;

        Vector3[] _CachedOperatingPositions;
        Vector3[] CachedOperatingPositions
        {
            get
            {
                if (this._CachedOperatingPositions == null)
                {
                    var benchglobal = this.Workstation.Global;
                    var cell = this.Actor.Map.GetCell(benchglobal);
                    this._CachedOperatingPositions = cell.Block.GetOperatingPositions(cell, benchglobal).ToArray();
                }
                return this._CachedOperatingPositions;
            }
        }


        protected override IEnumerable<Behavior> GetSteps()
        {
            bool noOperatingPositions() // 
            {
                // fail if no available operating positions
                /// CHECK MULTIPLE POSITIONS OR ONLY FRONT?
                /// WILL I HAVE WORKSTATIONS WITH MULTIPLE OPERATING POSITIONS???

                var map = this.Actor.Map;
                var front = map.GetFrontOfBlock(this.Workstation.Global);
                if (!map.IsStandableIn(front))
                {
                    "no operating position available".ToConsole();
                    return true;
                } 
                return false;// || !this.Actor.CanReserve(front);

                /// WILL I HAVE WORKSTATIONS WITH MULTIPLE OPERATING POSITIONS???
                //for (int i = 0; i < this.CachedOperatingPositions.Length; i++)
                //{
                //    var pos = this.CachedOperatingPositions[i];
                //    if (map.IsStandableIn(pos) && this.Actor.CanReserve(pos))
                //    {
                //        return false;
                //    }
                //}
                //"no available operation positions".ToConsole();
                //return true;
                /// WILL I HAVE WORKSTATIONS WITH MULTIPLE OPERATING POSITIONS???
            };
            this.FailOn(noOperatingPositions);
            bool orderIncompletable()
            {
                //return !this.Task.Order.IsActive(this.Actor.Map);
                var val = !this.Task.Order.IsActive() || !this.Task.Order.IsCompletable();
                if(val)
                {
                    "order incompletable".ToConsole();
                }
                return val;
            }
            //bool failOnInvalidWorkstation() { return !(this.Workstation.GetBlock() is BlockWorkstation); };
            bool failOnInvalidWorkstation() { return !(this.Workstation.GetBlockEntity()?.HasComp<BlockEntityCompWorkstation>() ?? false); };

            var nextIngredient = BehaviorHelper.ExtractNextTargetAmount(IngredientIndex);
            yield return nextIngredient;
            yield return new BehaviorGetAtNewNew(IngredientIndex).FailOnUnavailableTarget(IngredientIndex).FailOn(failOnInvalidWorkstation).FailOn(orderIncompletable).FailOn(noOperatingPositions);
            //yield return new BehaviorInteractionNew(IngredientIndex, () => new InteractionHaul(this.Task.GetAmount(IngredientIndex))).FailOnUnavailableTarget(IngredientIndex).FailOn(failOnInvalidWorkstation).FailOn(orderIncompletable).FailOn(noOperatingPositions);
            yield return BehaviorHaulHelper.StartCarrying(IngredientIndex)
                //.FailOnUnavailableTarget(IngredientIndex)
                .FailOn(failOnInvalidWorkstation)
                .FailOn(orderIncompletable)
                .FailOn(noOperatingPositions);
            //yield return BehaviorReserve.ReserveCarried(); // reserve carried object in case the object was split from the previous target
            yield return BehaviorHelper.JumpIfNextCarryStackable(nextIngredient, IngredientIndex);

            yield return new BehaviorCustom()
            {
                InitAction = () =>
                    this.Task.SetTarget(AuxiliaryIndex, new TargetArgs(this.Actor.Map, this.Workstation.Global.Above()))
            };
            yield return new BehaviorGetAtNewNew(AuxiliaryIndex).FailOn(failOnInvalidWorkstation).FailOn(orderIncompletable).FailOn(noOperatingPositions);
            yield return new BehaviorInteractionNew(AuxiliaryIndex, ()=>new UseHauledOnTarget()).FailOn(failOnInvalidWorkstation).FailOn(orderIncompletable).FailOn(noOperatingPositions);
            yield return BehaviorHelper.JumpIfMoreTargets(nextIngredient, IngredientIndex);
            ///NO!!!! if they collected more than one item in the same stack, this code will only add the last (disposed) target that was merged to the stack
            ///add code in usehauledontarget interaction instead
            //yield return new BehaviorCustom()
            //{
            //    InitAction = () => {
            //        var placedObj = this.Ingredient;
            //        var task = this.Task;
            //        var existing = task.PlacedObjects.Find(t => t.Object.ID == placedObj.Object.ID);
            //        if (existing == null)
            //            task.PlacedObjects.Add(new ObjectAmount(placedObj.Object));
            //        else
            //            existing.Amount += placedObj.Object.StackSize;
            //    }
            //};



            //yield return new BehaviorCustom()
            //{
            //    InitAction = () =>
            //        this.Task.SetTarget(AuxiliaryIndex, new TargetArgs(this.Actor.Map, this.Workstation.Global + this.Actor.Map.GetCell(this.Workstation.Global).Front()))
            //};


            // choose operating position
            /// WILL I HAVE WORKSTATIONS WITH MULTIPLE OPERATING POSITIONS???
            yield return new BehaviorCustom()
            {
                InitAction = () => {

                    /// WILL I HAVE WORKSTATIONS WITH MULTIPLE OPERATING POSITIONS???
                    //var chosen = this.CachedOperatingPositions.Where(g => this.Actor.Map.IsStandableIn(g) && this.Actor.CanReserve(g)).OrderBy(g => Vector3.DistanceSquared(g, this.Actor.Global)).FirstOrDefault();
                    //if (chosen == null)
                    //{
                    //    this.Actor.EndCurrentTask();
                    //    return;
                    //}
                    /// WILL I HAVE WORKSTATIONS WITH MULTIPLE OPERATING POSITIONS???
                    var map = this.Actor.Map;
                    var front = map.GetFrontOfBlock(this.Workstation.Global);
                    this.Task.SetTarget(AuxiliaryIndex, new TargetArgs(map, front));
                }
            };

            //yield return BehaviorReserve.Reserve(AuxiliaryIndex); // reserved operating position at init
            //yield return BehaviorHelper.ExtractNextTarget(AuxiliaryIndex);
            yield return new BehaviorGetAtNewNew(AuxiliaryIndex, PathingSync.FinishMode.Exact).FailOnUnavailablePlacedItems().FailOn(orderIncompletable);
            //yield return new BehaviorInteractionNew(WorkstationIndex, () => new InteractionCraftNew(this.Task.Order.Reaction, this.Task.MaterialsUsed)).FailOnUnavailablePlacedItems();
            //yield return new BehaviorInteractionNew(WorkstationIndex, () => new InteractionCraftNew(this.Task.Order.ID, this.Task.IngredientsUsed, this.Task.PlacedObjects.Select(r => r.Object.InstanceID).ToList())).FailOnUnavailablePlacedItems().FailOn(orderIncompletable);
            yield return new BehaviorInteractionNew(WorkstationIndex, () => new InteractionCraftNew(this.Task.Order.ID, this.Task.PlacedObjects)).FailOnUnavailablePlacedItems().FailOn(orderIncompletable);


            //yield return new BehaviorInteractionNew(WorkstationIndex, () => new InteractionCraftNew(this.Task.Order, this.Task.MaterialsUsed)).FailOnUnavailablePlacedItems();
            //if (!this.Task.Order.HaulOnFinish)
            //    yield break;
            // assign a new haul behavior directly to the actor instead of adding the steps here?
            yield return new BehaviorCustom()
            {
                InitAction = () =>
                {
                    this.Actor.Reserve(this.Task.Product, -1);
                    if (this.Task.Order.HaulOnFinish && Towns.StockpileManager.GetBestStoragePlace(this.Actor, this.Task.Product.Object as Entity, out TargetArgs target))
                    {
                        this.Task.SetTarget(WorkstationIndex, target);
                        this.Task.SetTarget(IngredientIndex, this.Task.Product);
                    }
                    else if (HaulHelper.TryFindNearbyPlace(this.Actor, this.Task.Product.Object, this.Task.GetTarget(WorkstationIndex).Global, out TargetArgs nearby))
                    {
                        this.Task.SetTarget(WorkstationIndex, nearby);
                        this.Task.SetTarget(IngredientIndex, this.Task.Product);
                    }
                    else
                        this.Actor.EndCurrentTask();
                }
            };
            //.FailOn(() => 
            //!this.Task.Order.HaulOnFinish);

            bool deliverFail()
            {
                //var o = this.Actor.GetCarried();
                //if (o == null)
                //return false;

                if (this.Task.Order.HaulOnFinish)
                {
                    return !HaulHelper.IsValidStorage(this.Task.GetTarget(WorkstationIndex), this.Actor.Map, this.Task.Product.Object);
                }
                else
                {
                    return !this.IsValidStorage(this.Task.GetTarget(WorkstationIndex));
                }

                //var isvalid = HaulHelper.IsValidStorage(this.Task.GetTarget(WorkstationIndex), this.Actor.Map, this.Task.Product.Object);
                //if (!isvalid)
                //{
                //}

                //return !isvalid;
            };
            //yield return BehaviorReserve.Reserve(WorkstationIndex);
            yield return new BehaviorGetAtNewNew(IngredientIndex).FailOn(deliverFail).FailOnUnavailableTarget(IngredientIndex);
            yield return BehaviorHelper.StartCarrying(IngredientIndex).FailOn(deliverFail).FailOnUnavailableTarget(IngredientIndex);// new BehaviorInteractionNew(IngredientIndex, () => new Haul()).FailOnUnavailableTarget(IngredientIndex);
            yield return new BehaviorGetAtNewNew(WorkstationIndex).FailOn(deliverFail);
            yield return BehaviorHelper.PlaceCarried(WorkstationIndex).FailOn(deliverFail);
        }
        protected override bool InitExtraReservations()
        {
            var task = this.Task;
            var actor = this.Actor;
            var benchGlobal = this.Workstation.Global;
            var benchGlobalAbove = this.Workstation.Global.Above();
            var operatingPos = actor.Map.GetFrontOfBlock(benchGlobal);
            var operatingPosBelow = operatingPos.Below();
            return task.ReserveAll(actor, IngredientIndex)
                //&& task.Reserve(actor, WorkstationIndex)
                && actor.Reserve(benchGlobal)
                && actor.Reserve(benchGlobalAbove)
                && actor.Reserve(operatingPos)
                && actor.Reserve(operatingPosBelow);
                //&& this.Task.Reserve(this.Actor, AuxiliaryIndex); // dont reserve auxiliaryindex because we interchange it during the bhav
                //&& this.Task.ReserveAll(this.Actor, AuxiliaryIndex);
        }
        bool IsValidStorage(TargetArgs target)
        {
            if (target.HasObject)
            {
                return !target.Object.IsForbidden && target.Object.CanAbsorb(this.Task.Product.Object);
            }
            else
            {
                var map = this.Actor.Map;
                var global = target.Global;
                return map.IsSolid(global.Below()) && map.IsEmpty(global);
            }
        }
    }

}
