using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components.Interactions;

namespace Start_a_Town_.AI
{
    class AIHauling
    {
        static public List<AIInstruction> Haul(GameObject actor, Vector3 initialPosition, Func<GameObject, bool> filter, int quantity, TargetArgs target, List<GameObject> handledItems)
        {
            var list = new List<AIInstruction>();

            var potentialStacks = (from stack in actor.Net.Map.GetObjects()
                                   where stack.Physics.Size != Components.ObjectSize.Immovable
                                   where !handledItems.Contains(stack)
                                   where filter(stack)
                                   select stack).ToList();
            var amountFound = 0;
            foreach (var i in potentialStacks)
            {
                amountFound += i.StackSize;
                if (amountFound >= quantity)
                    break;
            }
            if (amountFound < quantity)
                return list;

            var closestItem = potentialStacks.OrderBy(i => Vector3.DistanceSquared(i.Global, initialPosition)).FirstOrDefault();
            if (closestItem == null)
                return list;
            var itemid = closestItem.IDType;

            var stackmax = closestItem.StackMax;
            //int amountMax;

            //start gathering same type of items near the closest item, until we hit max stack or until we hit target quantity
            //amountFound = Math.Min(amountMax, stackmax);
            //var amountRemaining = amountMax;
            var amountRemaining = quantity;
            var amountPicked = 0;
            //if (amountFound < stackmax)
            //{
            //    //just haul this one item
            //}
            //else
            //{
            // otherwise look for similar nearby items to combine 
            //var handledItems = new List<GameObject>();

            // 1) gather items near the first item, or 2) look for new nearby items each time we gather the next one?
            // let's try 2
            var currentItem = closestItem;
            var currentGlobal = currentItem.Global;
            while (currentItem != null && amountRemaining > 0)
            {
                var amountToPick = Math.Min(amountRemaining, currentItem.StackSize);
                amountRemaining -= amountToPick;
                amountPicked += amountToPick;
                var instr = new AIInstruction(new TargetArgs(currentItem), new InteractionHaul(amountToPick));
                list.Add(instr);
                if (amountPicked == currentItem.StackSize)
                {
                    list.Add(new AIInstruction(target, new UseHauledOnTarget()));
                    amountPicked = 0;
                    handledItems.Add(currentItem); // fixed? TODO: handle case where the next thing to haul is of the same type but there are still items in the last handled stack!
                    // if we hauled our current stack to the target, look for the rest of the items near the target, otherwise keep looking near the currentle found item
                    currentGlobal = target.Global;
                }
                else
                    currentGlobal = currentItem.Global;
                //var nearbyItems = new Queue<GameObject>(currentItem.GetNearbyObjects(r => r < 10, o => o.ID == itemid));
                // todo: optimize this
                
                //var nearbyItems = currentItem.GetNearbyObjects(r => r < 10, o => o.ID == itemid).Where(o => !handledItems.Contains(o)).Where(o => !handledItems.Contains(o)).OrderBy(o => Vector3.DistanceSquared(o.Global, currentItem.Global));
                var nearbyItems = currentItem.Map.GetNearbyObjects(currentGlobal, r => r < 10, o => o.IDType == itemid).Where(o => !handledItems.Contains(o)).Where(o => !handledItems.Contains(o)).OrderBy(o => Vector3.DistanceSquared(o.Global, currentItem.Global));
                currentItem = nearbyItems.FirstOrDefault();
            }
            if (amountPicked > 0)
                list.Add(new AIInstruction(target, new UseHauledOnTarget()));
            //}
            return list;
        }

        static public List<AIInstruction> Haul(GameObject actor, Vector3 initialPosition, int itemID, int quantity, TargetArgs target, List<GameObject> handledItems)
        {
            return Haul(actor, initialPosition, o => (int)o.IDType == itemID, quantity, target, handledItems);
        }
    }
}

