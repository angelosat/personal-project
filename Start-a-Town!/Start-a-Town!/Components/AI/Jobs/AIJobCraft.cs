using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.Modules.Crafting;
using Start_a_Town_.Blocks;
using Start_a_Town_.Crafting;

namespace Start_a_Town_.AI
{
    class AIJobCraft : AIJob
    {
        CraftOperationNew Operation;
        GameObject Actor;
        Vector3 WorkStation;

        public AIJobCraft(CraftOperationNew crafting)
        {
            this.Operation = crafting;
        }
        //public override void Initialize(GameObject actor, CraftOperationNew crafting)
        //{
        //    var entity = actor.Map.GetBlockEntity(crafting.WorkstationEntity) as BlockEntityWorkstation;
        //    entity.CurrentOperation = crafting;
        //}
        public AIJobCraft()
        {

        }
        public AIJobCraft(GameObject actor, Vector3 station)
        {
            this.Actor = actor;
            this.WorkStation = station;
        }
        public override void Reserve(GameObject actor)
        {
            base.Reserve(actor);
            //var entity = actor.Map.GetBlockEntity(this.Operation.WorkstationEntity) as BlockEntityWorkstation;
            var entity = actor.Map.GetBlockEntity(this.WorkStation) as BlockEntityWorkstation;
            entity.CurrentOperation = this.Operation;
        }
        internal override void Cancel()
        {
            base.Cancel();
            this.UnreserveStation();
        }
        public override void Dispose(GameObject actor)
        {
            base.Dispose(actor);
            this.UnreserveStation();
        }
        
        private void UnreserveStation()
        {
            //var entity = this.Actor.Map.GetBlockEntity(this.Operation.WorkstationEntity) as BlockEntityWorkstation;
            var entity = this.Actor.Map.GetBlockEntity(this.WorkStation) as BlockEntityWorkstation;
            entity.CurrentOperation = null;
            entity.CurrentWorker = null; //shit
        }
        public override object Clone()
        {
            //return new AIJobCraft(this.Operation);
            return new AIJobCraft(this.Actor, this.WorkStation);
        }

        //static public AIJobCraft Create(GameObject actor, Vector3 closestStation, CraftOrderNew order)
        //{
        //    AIJobCraft job = new AIJobCraft(actor, closestStation);
        //    return job;
        //    //var reagentReqs = order.Reaction.Reagents;
        //    //var nearbyItems = actor.Map.GetObjects();
        //    //var materialsFound = false;
        //    //List<GameObject> handled = new List<GameObject>();
        //    ////var product = new Reaction.Product.ProductMaterialPair();
        //    //var matsfound = new Dictionary<string, int>();
        //    //// either combine haul tasks of same material, or mark handled items as handled so as to not haul them again when the next reagent is of the same material as the last one
        //    //var handledStacks = new List<GameObject>();
        //    //var startingPosition = actor.Global;
        //    //foreach (var reagent in reagentReqs)
        //    //{
        //    //    materialsFound = false;
        //    //    //foreach (var item in nearbyItems)
        //    //    //{
        //    //    //    if (handled.Contains(item))
        //    //    //        continue;
        //    //    //    handled.Add(item);
        //    //    //    if (reagent.Filter(item))
        //    //    //    {
        //    //    //        // set item as reaction reagent
        //    //    //        matsfound.Add(reagent.Name, (int)item.ID);
        //    //    //        job.AddStep(new AI.AIInstruction(new TargetArgs(item), new Components.Interactions.PickUp()));
        //    //    //        job.AddStep(new AI.AIInstruction(new TargetArgs(closest), new InteractionAddMaterial()));
        //    //    //        materialsFound = true;
        //    //    //        break;
        //    //    //    }
        //    //    //}
        //    //    //if (!materialsFound)
        //    //    //    return list;

        //    //    var haulinstructions = AIHauling.Haul(actor, startingPosition, reagent.Filter, 1, new TargetArgs(closestStation), handledStacks);
        //    //    // hacky way of figuring out which materials we're gonna use based on the first available material the ai is gonna haul
        //    //    if (haulinstructions.Count > 0)
        //    //    {
        //    //        matsfound.Add(reagent.Name, (int)haulinstructions.First().Target.Object.IDType);
        //    //        job.Instructions = job.Instructions.Concat(haulinstructions).ToList();
        //    //        startingPosition = closestStation;
        //    //        materialsFound = true;
        //    //    }
        //    //}
        //    //if (!materialsFound)
        //    //    return null;

        //    //// create a craftingoperation in the workstation with the reagents we found
        //    ////var product = order.Product.Products.First().GetProduct(order.Product, matsfound);

        //    //// TODO: fix making the reagents dictionary during hauling to pass to the interaction
        //    //// prefer materials close to the workstation or close to the agent at the moment he gets assigned the job? the latter might cause the same material being used for all crafting reagents,
        //    //// because a single stack of it will be hauled in one trip and will be preffered. 
        //    //var station = actor.Map.GetBlock(closestStation) as BlockWorkstation;// as IBlockWorkstation;

        //    //var i = station.GetCraftingInteraction(order.Reaction, matsfound);
        //    //AIInstruction instr = new AIInstruction(new TargetArgs(closestStation), i);
        //    //job.AddStep(instr);
        //    //var entity = actor.Map.GetBlockEntity(closestStation) as BlockEntityWorkstation;
        //    //entity.CurrentWorker = actor; // shit
        //    //job.Reserve(actor);
        //    //return job; 
        //}
    }
}
