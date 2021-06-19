using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.Components;
using Start_a_Town_.AI.Behaviors;
using Start_a_Town_.AI;

namespace Start_a_Town_
{
    class TaskBehaviorSleepingNew : BehaviorPerformTask
    {
        static public TargetIndex BedIndex = TargetIndex.A;
        //int BedIndex = 0;
        //TargetArgs Bed { get { return this.Task.Targets[this.BedIndex]; } }
        //public BehaviorTaskSleepingNew(TaskSleeping taskSleeping)
        //{
        //    // TODO: Complete member initialization
        //    this.Task = taskSleeping;
        //}
        public override string Name
        {
            get
            {
                return "Sleeping";
            }
        }
        public TaskBehaviorSleepingNew()
        {

        }
        protected override IEnumerable<Behavior> GetSteps()
        {
            //this.FailOn(IsEnergyFull);
            //yield return new BehaviorCustom() { InitAction = () => {
            //    var global = this.Task.GetTarget(BedIndex).Global;
            //    this.Task.SetTarget(TargetIndex.B, new TargetArgs(this.Actor.Map.GetCell(global).GetOperatingPositions().Select(p => global + p).Where(this.Actor.CanReach).First()));
            //} };
            yield return new BehaviorGetAtNewNew(TargetIndex.B, PathingSync.FinishMode.Exact);//, 1);
            //yield return new BehaviorInteractionNew(TargetIndex.A, () => new Blocks.Bed.InteractionSleepInBed());

            yield return new BehaviorCustom()
            {
                Mode = BehaviorCustom.Modes.Continuous,
                Init = (a, s) => this.Actor.Interact(new Blocks.Bed.InteractionSleepInBed(), this.Task.TargetA),
                SuccessCondition = a => IsEnergyFull()
            };
            //yield return new BehaviorCustom() { Init = (a, t) => AIManager.Interrupt(this.Actor) };
            yield return new BehaviorCustom() { Init = (a, t) => AIManager.EndInteraction(this.Actor, true) };
        }
        protected IEnumerable<Behavior> GetStepsNotSoGood()
        {
            var sleeping = new Blocks.Bed.InteractionSleepInBed();
            var global = this.Task.TargetA.Global;
            var operatingPosition = this.Actor.Map.GetCell(global).GetOperatingPositions().Select(p=>global + p).Where(this.Actor.CanReach).First();
            //var target = this.Task.Target;
            var target = new TargetArgs(this.Actor.Map, operatingPosition);
            //yield return new BehaviorGetAtNewNew(target, 1);
            yield return new BehaviorGetAtNewNew(target, PathingSync.FinishMode.Exact);//, 1);

            //yield return new BehaviorCustom() { Mode = BehaviorCustom.Modes.Continuous, Init = (a, s) => AIManager.Interact(this.Actor, sleeping, this.Task.Target), SuccessCondition = IsEnergyFull };
            yield return new BehaviorCustom() { Mode = BehaviorCustom.Modes.Continuous, Init = (a, s) => this.Actor.Interact(sleeping, this.Task.TargetA), SuccessCondition = a=> IsEnergyFull() };
            yield return new BehaviorCustom() { Init = (a, s) => AIManager.EndInteraction(this.Actor) };
        }
        protected IEnumerable<Behavior> GetStepsGood()
        {
            yield return new BehaviorSelector(
                    new BehaviorCondition((a, s) => this.IsInBed()),
                    //new BehaviorGoInteract("target", 1, new Blocks.Bed.InteractionToggleSleep()));
                    new BehaviorSequence(
                        new BehaviorGetAtNewNew(this.Task.TargetA),
                        new BehaviorInteractionNew(this.Task.TargetA, new Blocks.Bed.InteractionStartSleep())));

            //yield return new BehaviorQueue(
            //        new BehaviorCondition((a, s) => this.IsEnergyFull()),
            //        new BehaviorGoInteract("target", 1, new Blocks.Bed.InteractionToggleSleep()));

            //yield return new BehaviorUntilFail()
            //    new BehaviorCondition((a, s) => this.IsEnergyFull()),
            //        new BehaviorInteractionNew(this.Task.Target, new Blocks.Bed.InteractionToggleSleep()));
            yield return new BehaviorCustom() { SuccessCondition = a=>IsEnergyFull(), Mode = BehaviorCustom.Modes.Continuous };
            yield return new BehaviorInteractionNew(this.Task.TargetA, new Blocks.Bed.InteractionStopSleep());
        }

        bool IsInBed()
        {
            var bedglobal = this.Task.TargetA.Global;
            var bedentity = this.Actor.Map.GetBlock(bedglobal).GetBlockEntity(this.Actor.Map, bedglobal) as BlockBedEntity;
            return bedentity.CurrentOccupant == this.Actor.RefID;
        }
        bool IsEnergyFull()
        {
            var needenergy = this.Actor.GetNeed(NeedDef.Energy);// NeedsComponent.GetNeed(this.Actor, Need.Types.Energy);

            //needenergy.SetValue(needenergy.Value + 1, parent);
            //if (this.Tick == this.TickMax)
            //{
            //    NeedsComponent.ModifyNeed(parent, Components.Needs.Need.Types.Energy, 1);
            //    this.Tick = 0;
            //}
            //else
            //    this.Tick++;
            return needenergy.Percentage == 1;
        }
        //public override bool ReserveTargets()
        //{
        //    return this.Actor.Reserve(this.Bed);
        //}
    }
}
