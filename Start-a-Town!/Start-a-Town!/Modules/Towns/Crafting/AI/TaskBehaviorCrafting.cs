using System.Collections.Generic;
using System.Linq;
using Start_a_Town_.AI.Behaviors;
using Start_a_Town_.Crafting;

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

        IntVec3[] _CachedOperatingPositions;
        IntVec3[] CachedOperatingPositions
        {
            get
            {
                if (this._CachedOperatingPositions == null)
                {
                    var benchglobal = this.Workstation.Global;
                    var cell = this.Actor.Map.GetCell(benchglobal);
                    this._CachedOperatingPositions = cell.Block.GetInteractionSpots(cell, benchglobal).ToArray();
                }
                return this._CachedOperatingPositions;
            }
        }

        protected override IEnumerable<Behavior> GetSteps()
        {
            
            this.FailOn(noOperatingPositions);
            
            var nextIngredient = BehaviorHelper.ExtractNextTargetAmount(IngredientIndex);
            yield return nextIngredient;
            yield return new BehaviorGetAtNewNew(IngredientIndex).FailOnUnavailableTarget(IngredientIndex).FailOn(failOnInvalidWorkstation).FailOn(orderIncompletable).FailOn(noOperatingPositions);
            yield return BehaviorHaulHelper.StartCarrying(IngredientIndex)
                .FailOn(failOnInvalidWorkstation)
                .FailOn(orderIncompletable)
                .FailOn(noOperatingPositions);
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
            yield return new BehaviorGrabTool();
            yield return new BehaviorGetAtNewNew(AuxiliaryIndex, PathingSync.FinishMode.Exact).FailOn(placedObjectsChanged).FailOn(orderIncompletable);
            yield return new BehaviorInteractionNew(WorkstationIndex, () => new InteractionCrafting(this.Task.Order, this.Task.PlacedObjects)).FailOn(placedObjectsChanged).FailOn(orderIncompletable);
            yield return new BehaviorInteractionNew(TargetIndex.Tool, () => new Equip()); // unequip the tool before hauling product
            // assign a new haul behavior directly to the actor instead of adding the steps here?
            yield return new BehaviorCustom()
            {
                InitAction = () =>
                {
                    this.Actor.Reserve(this.Task.Product, -1);
                    if (this.Task.Order.HaulOnFinish && StockpileAIHelper.GetAllValidStoragePlacesNoReserveCheckLazy(this.Actor, this.Task.Product.Object as Entity) is var places && places.Any())// ; Towns.StockpileManager.GetBestStoragePlace(this.Actor, this.Task.Product.Object as Entity, out TargetArgs target))
                    {
                        var target = places.First();
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
           
            yield return new BehaviorGetAtNewNew(IngredientIndex).FailOn(deliverFail).FailOnUnavailableTarget(IngredientIndex);
            yield return BehaviorHelper.StartCarrying(IngredientIndex).FailOn(deliverFail).FailOnUnavailableTarget(IngredientIndex);
            yield return new BehaviorGetAtNewNew(WorkstationIndex).FailOn(deliverFail);
            yield return BehaviorHelper.PlaceCarried(WorkstationIndex).FailOn(deliverFail);

            bool orderIncompletable()
            {
                //return !this.Task.Order.IsActive(this.Actor.Map);
                var val = !this.Task.Order.IsActive || !this.Task.Order.IsCompletable();
                if (val)
                {
                    "order incompletable".ToConsole();
                }
                return val;
            }
            bool failOnInvalidWorkstation()
            {
                return !(this.Workstation.BlockEntity?.HasComp<BlockEntityCompWorkstation>() ?? false);
            };
            bool deliverFail()
            {
                if (this.Task.Order.HaulOnFinish)
                    return !HaulHelper.IsValidStorage(this.Task.GetTarget(WorkstationIndex), this.Actor.Map, this.Task.Product.Object);
                else
                    return !this.IsValidStorage(this.Task.GetTarget(WorkstationIndex));
            };
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
                return false;
            };
            bool placedObjectsChanged()
            {
                foreach (var t in this.Task.PlacedObjects)
                {
                    if (t.Object.IsDisposed)
                        return true;
                    if (t.Object.IsForbidden)
                        return true;
                    if (t.Object.CellIfSpawned != this.Workstation.Global.Above())
                        return true;
                }
                return false;
            }
        }
        protected override bool InitExtraReservations()
        {
            var task = this.Task;
            var actor = this.Actor;
            var benchGlobal = (IntVec3)this.Workstation.Global;
            var benchGlobalAbove = benchGlobal.Above;
            var operatingPos = actor.Map.GetFrontOfBlock(benchGlobal);
            var operatingPosBelow = operatingPos.Below;
            return task.ReserveAll(actor, IngredientIndex)
                && actor.Reserve(benchGlobal)
                && actor.Reserve(benchGlobalAbove)
                && actor.Reserve(operatingPos)
                && actor.Reserve(operatingPosBelow);
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
