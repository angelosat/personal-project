using System.Collections.Generic;
using Start_a_Town_.AI.Behaviors.ItemOwnership;
using Start_a_Town_.AI.Behaviors;

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
            new TaskGiverConstructing(),
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
            new TaskGiverHaulToStockpile(),
            new TaskGiverTradingOverCounter(),
            new TaskGiverOfferQuest(),
            new TaskGiverWorkplace()
        };

        static readonly public List<TaskGiver> VisitorTaskGivers = new()
        { 
            new TaskGiverVisitorRentRoom(),
            new TaskGiverBeTalkedTo(),
            new TaskGiverQuestComplete(),
            new TaskGiverGetQuests(),
            new TaskGiverTavernCustomer(),
            new TaskGiverDepart()
        };

        protected virtual AITask TryAssignTask(Actor actor) { return null; }
        
        public TaskGiverResult FindTask(Actor actor)
        {
            var task = TryAssignTask(actor);
            return task != null ? new TaskGiverResult(task, this) : TaskGiverResult.Empty;
        }
        
        protected static void FindTool(Actor actor, AITask task, ToolAbilityDef skill)
        {
            var equipped = actor.GetEquipmentSlot(GearType.Mainhand);//.Object;
            if (equipped != null && equipped.ProvidesSkill(skill))
                task.Tool = new TargetArgs(equipped);
            else
                task.Tool = TaskHelper.FindItemAnywhere(actor, o => o.ProvidesSkill(skill));
        }
        
        public virtual AITask TryTaskOn(Actor actor, TargetArgs target, bool ignoreOtherReservations = false) { return null; }
    }
}
