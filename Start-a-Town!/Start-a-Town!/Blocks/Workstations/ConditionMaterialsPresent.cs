using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.Components.Interactions;
using Start_a_Town_.Blocks;
using Start_a_Town_.AI;

namespace Start_a_Town_.Crafting
{
    class MaterialsPresent : ScriptTaskCondition
    {
        public override bool Condition(GameObject actor, TargetArgs target)
        {
            var global = target.Global;
            var entity = actor.Map.GetBlockEntity(global) as BlockEntityWorkstation;// Entity;
            if (entity == null)
                throw new Exception();
            var craft = entity.GetCurrentOrder();// entity.GetQueuedOrders().First().Craft;
            return entity.MaterialsPresent(craft);
        }
        //public override bool AITrySolve(GameObject agent, TargetArgs target, AIState state, List<AIInstruction> solution)
        //{
        //    //throw new NotImplementedException();
        //    var global = target.Global;

        //    //var block = agent.Map.GetBlock(global) as BlockWorkbench;
        //    //if (block == null)
        //    //    throw new Exception();

        //    var entity = agent.Map.GetBlockEntity(global) as BlockEntityWorkstation;
        //    if (entity == null)
        //        throw new Exception();
        //    //var craft = entity.CurrentOrder;

        //    var craft = entity.GetOrders(agent.Net, target.Global).First().Craft;// agent.Map.GetTown().CraftingManager.GetOrdersFor(target.Global).First().Craft;

        //    foreach (var mat in craft.Materials)
        //    {
        //        var items = (from i in agent.Map.GetObjects() where (int)i.IDType == mat.ObjectID select i).ToList();
        //        var item = items.FirstOrDefault();
        //        if (item == null)
        //            return false;
        //        solution.Add(new AI.AIInstruction(new TargetArgs(item), new Haul()));
        //        solution.Add(new AI.AIInstruction(target, new InteractionAddMaterial()));//entity)));
        //    }
        //    //var instruction = new AI.AIInstruction(target, new InteractionAddMaterial(entity));
        //    return false;
        //}
        //public override bool AITrySolve(GameObject agent, TargetArgs target, AIState state, List<AIInstruction> solution)
        //{
        //    //throw new NotImplementedException();
        //    var global = target.Global;

        //    //var block = agent.Map.GetBlock(global) as BlockWorkbench;
        //    //if (block == null)
        //    //    throw new Exception();

        //    var entity = agent.Map.GetBlockEntity(global) as BlockEntityWorkstation;
        //    if (entity == null)
        //        throw new Exception();
        //    //var craft = entity.CurrentOrder;

        //    var orders = agent.Map.Town.CraftingManager.GetOrdersNew(global);
        //    var order = orders.First();

        //    // TEMP: check here for any nearby items that can be accepted as reagents
        //    var reagentReqs = order.Product.Reagents;
        //    var nearbyItems = agent.Map.GetObjects();
        //    var materialsFound = false;
        //    foreach(var reagent in reagentReqs)
        //    {
        //        foreach(var item in nearbyItems)
        //        {
        //            if(reagent.Filter(item))
        //            {
        //                solution.Add(new AI.AIInstruction(new TargetArgs(item), new PickUp()));
        //                solution.Add(new AI.AIInstruction(target, new InteractionAddMaterial()));
        //                materialsFound = true;
        //                continue;
        //            }
        //        }
        //    }
        //    if (!materialsFound)
        //        return false;
        //    return false;
        //    //var product = order.Product.pro
        //    var craft = entity.GetOrders(agent.Net, target.Global).First().Craft;// agent.Map.GetTown().CraftingManager.GetOrdersFor(target.Global).First().Craft;

        //    foreach (var mat in craft.Materials)
        //    {
        //        var items = (from i in nearbyItems where (int)i.ID == mat.ObjectID select i).ToList();
        //        var item = items.FirstOrDefault();
        //        if (item == null)
        //            return false;
        //        solution.Add(new AI.AIInstruction(new TargetArgs(item), new PickUp()));
        //        solution.Add(new AI.AIInstruction(target, new InteractionAddMaterial()));//entity)));
        //    }
        //    //var instruction = new AI.AIInstruction(target, new InteractionAddMaterial(entity));
        //    return false;
        //}
    }

}
