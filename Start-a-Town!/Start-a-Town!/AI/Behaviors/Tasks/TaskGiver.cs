using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.AI.Behaviors.ItemOwnership;
using Start_a_Town_.AI.Behaviors;
using Start_a_Town_.Components;

namespace Start_a_Town_
{
    abstract public class TaskGiver
    {
        static readonly public List<TaskGiver> EssentialTaskGivers = new()
        {
            new TaskGiverLeaveUnstandableCell(),
            new TaskGiverItemOwnership(),
            new TaskGiverEquipSelf(),
        };


        static readonly public List<TaskGiver> CitizenTaskGivers = new()
        {
            //new TaskGiverLeaveUnstandableCell(),
            new TaskGiverConstructing(), // TODO: SUPER SLOW (did i already fix it?)
            new TaskGiverRefueling(),
            new TaskGiverSwitchToggle(),
            new TaskGiverChopping(),
            new TaskGiverForaging(),
            new TaskGiverDigging(),
            new TaskGiverDeconstruct(),
            new TaskGiverTillingNew(),
            new TaskGiverSowingNew(),
            new TaskGiverHarvestingNew(),
            new TaskGiverCrafting(),
            new TaskGiverFarming(),
            new TaskGiverHaulToStockpileNew(),
            //new TaskGiverOfferGuidance(),
            //new TaskGiverTrading()
            new TaskGiverTradingOverCounter(),
            new TaskGiverOfferQuest(),
            new TaskGiverWorkplace()
            //new TaskGiverItemOwnership(),
        };

        static readonly public List<TaskGiver> VisitorTaskGivers = new()
        { 
            new TaskGiverVisitorRentRoom(),
            new TaskGiverBeTalkedTo(),
            new TaskGiverQuestComplete(),
            new TaskGiverGetQuests(),
            //new TaskGiverTownArrival(),
            //new TaskGiverBuy(),
            //new TaskGiverSell()
            new TaskGiverTavernCustomer(),
            new TaskGiverDepart()
            //new TaskGiverSellOverCounter(),
            //new TaskGiverBuyOverCounter()
        };


        protected virtual AITask TryAssignTask(Actor actor) { return null; }
        //public virtual Queue<AITask> TryAssignTasks(GameObject actor) 
        //{
        //    var queue = new Queue<AITask>();
        //    queue.Enqueue(this.TryAssignTask(actor));
        //    return queue;
        //}
        public TaskGiverResult FindTask(Actor actor)
        {
            var task = TryAssignTask(actor);
            return task != null ? new TaskGiverResult(task, this) : TaskGiverResult.Empty;
        }
        //public Queue<AITask> TryAssignTasks(GameObject actor)
        //{
        //    var queue = new Queue<AITask>();
        //    queue.Enqueue(this.TryAssignTask(actor));
        //    return queue;
        //}
        //protected virtual AITask TryCleanUp(GameObject actor) { return null; }
        //public TaskGiverResult CleanUp(GameObject actor)
        //{
        //    var task = TryCleanUp(actor);
        //    return task != null ? new TaskGiverResult(task, this) : TaskGiverResult.Empty;
        //}
        protected static void FindTool(Actor actor, AITask task, ToolAbilityDef skill)
        {
            var equipped = actor.GetEquipmentSlot(GearType.Mainhand);//.Object;
            if (equipped != null && equipped.ProvidesSkill(skill))
                task.Tool = new TargetArgs(equipped);
            else
                task.Tool = TaskHelper.FindItemAnywhere(actor, o => o.ProvidesSkill(skill));
        }
        //protected static void FindTool(Entity actor, Skill skill, AITask task, int targetID)
        //{
        //    var equipped = actor.GetEquipmentSlot(GearType.Mainhand).Object;
        //    if (equipped != null && equipped.ProvidesSkill(skill))
        //    {
        //        var t = new TargetArgs(equipped);
        //        task.SetTarget(targetID, t);
        //        task.SetTool(t);
        //    }
        //    var found = TaskHelper.FindItemAnywhere(actor, o => o.ProvidesSkill(skill));
        //    task.SetTarget(targetID, found);
        //    task.SetTool(found);
        //}
        public virtual AITask TryTaskOn(Actor actor, TargetArgs target, bool ignoreOtherReservations = false) { return null; }

    }
}
