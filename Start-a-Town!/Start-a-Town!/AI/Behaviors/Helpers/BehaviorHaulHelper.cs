﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.AI;
using Start_a_Town_.AI.Behaviors;
using Start_a_Town_.Towns;

namespace Start_a_Town_
{
    class BehaviorHaulHelper
    {
        static public Behavior FindNearbyHaulOpportunity(Behavior gotoBhav, TargetIndex itemIndex)
        {
            var bhav = new BehaviorCustom
            {
                InitAction = () =>
                {
                    var actor = gotoBhav.Actor;
                    var hauling = actor.GetHauled();
                    var task = actor.CurrentTask;
                    var desiredAmount = Math.Min(task.Count, hauling.StackAvailableSpace);
                    if (desiredAmount == 0)
                        return;
                    var map = actor.Map;
                    var item = task.GetTarget(itemIndex).Object;
                    var count = task.Count;

                    var potentialItems = HaulHelper.GetPotentialItemsNew(actor);
                    foreach (var pot in potentialItems)
                    {
                        if (!item.CanAbsorb(pot))
                            continue;
                    //if (!actor.CanReserve(pot))
                    //    continue;
                    var unreservedAmount = actor.GetUnreservedAmount(pot);
                        if (unreservedAmount == 0)
                            continue;
                        desiredAmount = Math.Min(desiredAmount, unreservedAmount);
                        var amount = Math.Min(pot.StackSize, desiredAmount);
                        actor.Reserve(pot, amount);
                        task.SetTarget(itemIndex, new TargetArgs(pot));
                        actor.CurrentTaskBehavior.JumpTo(gotoBhav);
                        actor.Net.Log.Write("found new haul opportunity");
                        return;
                    }
                }
            };
            return bhav;
        }

        static public Behavior StartCarrying(TargetIndex storageIndex)
        {
            var bhav = new BehaviorCustom() { Mode = BehaviorCustom.Modes.Continuous };
            TargetArgs target = null;
            Interaction interaction = null;
            int amountToPickUp = 0;
            bhav.PreInitAction = () =>
            {
                {
                    var actor = bhav.Actor;
                    var task = actor.CurrentTask;
                    target = task.GetTarget(storageIndex);
                    var hauled = actor.GetHauled();
                    var item = target.Object;
                    var reservedAmount = actor.GetReservedAmount(item);

                    // this is for testing purposes. i only end up using the reserved amount
                    var amountFromTask = task.GetAmount(storageIndex);
                    amountFromTask = amountFromTask == -1 ? item.StackSize : amountFromTask;
                    if (reservedAmount != amountFromTask)
                        //throw new Exception(); // TODO not sure i should be getting the haul amount from the reservation instead of the amount propert in the task
                        actor.Net.SyncReport($"Reserved amount [{reservedAmount}] different than target amount [{amountFromTask}] in [{actor.Name}]'s [{bhav.GetType()}] behavior");

                    //// the item stacksize might have been increased since the behavior initialization 
                    //if (amountFromTask > reservedAmount)
                    //    throw new Exception("target amount larger than reserved amount");
                    //// do i need to throw this? i do amountToPickUp = reservedAmount immediately below

                    amountToPickUp = reservedAmount;
                    interaction = new InteractionHaul(amountToPickUp);
                    if (amountToPickUp > item.StackSize)
                        throw new Exception();
                    actor.Interact(interaction, target);
                }
            };
            bhav.FailOn(() => interaction.State == Interaction.States.Failed);
            bhav.FailOnUnavailableTarget(storageIndex);
            bhav.SuccessCondition = a =>
            {
                if (interaction.IsFinished)// .State == Interaction.States.Finished)
                {
                    var actor = bhav.Actor;
                    var task = actor.CurrentTask;
                    var hauled = actor.GetHauled();
                    task.Count -= amountToPickUp;
                    //actor.Unreserve(target); // UNDONE ??? dont unreserve here because the ai might continue manipulating (placing/carrying) the item during the same behavior

                    if (target.Object != actor.Carried)
                    {
                        actor.Unreserve(target); // ACTUALLY UNRESERVE SOURCE STACK HERE IN CASE THE HAULED STACK IS SPLIT FROM THE SOURCE ONE
                        actor.Reserve(actor.Carried);
                        task.SetTarget(storageIndex, actor.Carried); // replacing task target with combined item because otherwise the behavior will fail since the old item is now disposed
                    }
                    return true;
                }
                return false;
            };
            //bhav.AddEndCondition(() =>
            //{
            //    //var interaction = actor.CurrentInteraction;
            //    if (interaction.State == Interaction.States.Failed)
            //        return BehaviorState.Fail;
            //    else if (interaction.State == Interaction.States.Finished)
            //    {
            //        var task = actor.CurrentTask;
            //        var hauled = actor.GetHauled();
            //        task.Count -= amountToPickUp;
            //        return BehaviorState.Success;
            //    }
            //    return BehaviorState.Running;
            //});
            return bhav;
        }
        static public Behavior DropInStorage(Behavior gotoBhav, TargetIndex storageIndex)
        {
            var bhav = new BehaviorCustom() { Mode = BehaviorCustom.Modes.Continuous };
            Interaction interaction = null;
            GameObject hauledObj = null;
            //bhav.FailOnNotCarrying();
            bhav.PreInitAction = () =>
            {
                {
                    var actor = gotoBhav.Actor;
                    interaction = new UseHauledOnTargetNew();
                    hauledObj = actor.GetHauled();
                    var task = actor.CurrentTask;
                    var target = task.GetTarget(storageIndex);
                    actor.Interact(interaction, target);
                }
            };
            bhav.AddEndCondition(() =>
            {
                var actor = gotoBhav.Actor;
                //var interaction = actor.CurrentInteraction;
                if (interaction.State == Interaction.States.Failed)
                    return BehaviorState.Fail;
                else if (interaction.State == Interaction.States.Finished)
                {
                    //actor.Net.Log.Write("successfully dropped item");
                    if (actor.GetHauled() is not Entity hauled)//!(hauled?.StackSize > 0))
                        return BehaviorState.Success;
                    if (!hauledObj.IsDisposed)
                        actor.Unreserve(hauledObj);
                    var task = actor.CurrentTask;
                    var target = task.GetTarget(storageIndex);
                    actor.Unreserve(target);
                    var cell = target.Global;
                    var targets = StockpileManager.GetMoreValidStoragePlaces(actor, hauled, cell);
                    foreach (var tar in targets)
                    {
                        if (tar.HasObject && !tar.Object.CanAbsorb(hauled))
                            continue;
                        if (!actor.CanReserve(tar))
                            continue;
                        actor.Reserve(tar, 1);
                        task.SetTarget(storageIndex, tar);
                        actor.CurrentTaskBehavior.JumpTo(gotoBhav);
                        //actor.Net.Log.Write("found next storage place " + tar.ToString());
                        return BehaviorState.Running;
                    }
                }
                return BehaviorState.Running;
            });
            return bhav;
        }
        static public Behavior DropInStorage(TargetIndex storageIndex)
        {
            var bhav = new BehaviorCustom() { Mode = BehaviorCustom.Modes.Continuous };
            //Interaction interaction = null;
            GameObject hauledObj = null;
            //bhav.FailOnNotCarrying();
            bhav.PreInitAction = () =>
            {
                {
                    var actor = bhav.Actor;
                    var interaction = new UseHauledOnTargetNew();
                    hauledObj = actor.GetHauled();
                    var task = actor.CurrentTask;
                    var target = task.GetTarget(storageIndex);
                    actor.Interact(interaction, target);
                }
            };
            bhav.AddEndCondition(() =>
            {
                var actor = bhav.Actor;
                var interaction = actor.CurrentInteraction;
                if (interaction.State == Interaction.States.Failed)
                    return BehaviorState.Fail;
                else if (interaction.State == Interaction.States.Finished)
                {
                    actor.Net.Log.Write("successfully dropped item");
                    if (actor.GetHauled() is not Entity hauled)//!(hauled?.StackSize > 0))
                        return BehaviorState.Success;
                    if (!hauledObj.IsDisposed)
                        actor.Unreserve(hauledObj);
                    var task = actor.CurrentTask;
                    var target = task.GetTarget(storageIndex);
                    actor.Unreserve(target);
                    //var cell = target.Global;
                    //var targets = StockpileManager.GetMoreValidStoragePlaces(actor, hauled, cell);
                    //foreach (var tar in targets)
                    //{
                    //    if (tar.HasObject && !tar.Object.CanAbsorb(hauled))
                    //        continue;
                    //    if (!actor.CanReserve(tar))
                    //        continue;
                    //    actor.Reserve(tar, 1);
                    //    task.SetTarget(storageIndex, tar);
                    //    actor.CurrentTaskBehavior.JumpTo(gotoBhav);
                    //    actor.Net.Log.Write("found next storage place " + tar.ToString());
                    //    return BehaviorState.Running;
                    //}
                }
                return BehaviorState.Running;
            });
            return bhav;
        }
        static public Behavior JumpIfNextStorageFound(Behavior gotoBhav, TargetIndex storageIndex)
        {
            var bhav = new BehaviorCustom() { Mode = BehaviorCustom.Modes.Instant };
            Entity hauledObj = null;
            bhav.InitAction = () =>
            {
                var actor = gotoBhav.Actor;
                hauledObj = actor.GetHauled() as Entity;
                if (hauledObj == null)
                    return;
                var task = actor.CurrentTask;
                var target = task.GetTarget(storageIndex);
                var cell = target.Global.SnapToBlock();
                //var targets = StockpileManager.GetMoreValidStoragePlaces(actor, hauledObj, cell);
                var targets = GetMoreValidStoragePlaces(actor, hauledObj, cell);

                foreach (var tar in targets)
                {
                    if (tar.HasObject && !tar.Object.CanAbsorb(hauledObj))
                        continue;
                    if (!actor.CanReserve(tar))
                        continue;
                    actor.Reserve(tar, 1);
                    task.SetTarget(storageIndex, tar);
                    actor.CurrentTaskBehavior.JumpTo(gotoBhav);
                    actor.Net.Log.Write("found next storage place " + tar.ToString());
                    return;
                }
            };
            return bhav;
        }

        static public IEnumerable<TargetArgs> GetMoreValidStoragePlaces(Actor actor, Entity item, IntVec3 center)
        {
            var storage = item.Map.Town.ZoneManager.GetZoneAt<Stockpile>(center.Below);
            foreach (var spot in storage.GetPotentialHaulTargets(actor, item))
                yield return spot;
        }
    }
}
