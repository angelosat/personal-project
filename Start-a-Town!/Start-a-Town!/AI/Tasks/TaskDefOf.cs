using Start_a_Town_.AI.Behaviors;

namespace Start_a_Town_
{
    // TODO: load from external file?
    public static class TaskDefOf
    {
        static public TaskDef Crafting = new("Crafting", typeof(TaskBehaviorCrafting))
        {
            Format = "Force crafting at {0}", 
            GetPrimaryTarget = t => t.GetTarget(TaskBehaviorCrafting.WorkstationIndex)
        };

        static public TaskDef Hauling = new("Hauling", typeof(TaskBehaviorHaulToStockpileNew))
        {
            Format = "Force haul {0}",
            GetPrimaryTarget = t => t.GetTarget(TaskBehaviorHaulToStockpileNew.ItemInd)
        };

        static public TaskDef HaulAside = new("HaulAside", typeof(TaskBehaviorHaulAside))
        {
            Format = "Haul aside {0}",
            GetPrimaryTarget = t => t.GetTarget(TargetIndex.A)
        };

        static public TaskDef Construct = new("Construct", typeof(TaskBehaviorConstruct))
        {
            Format = "Construct {0}",
            GetPrimaryTarget = t => t.GetTarget(TargetIndex.A)
        };

        static public TaskDef DeliverMaterials = new("DeliverMaterials", typeof(TaskBehaviorDeliverMaterials))
        {
            Format = "Deliver materials to {0}",
            GetPrimaryTarget = t => t.GetTarget(TargetIndex.A)
        };

        static public TaskDef Sowing = new("Sowing", typeof(TaskBehaviorDeliverMaterials))
        {
            Format = "Sow {0}",
            GetPrimaryTarget = t => t.GetTarget(TargetIndex.A)
        };

        static public TaskDef Moving = new("Moving", typeof(TaskBehaviorLeaveUnstandableCell))
        {
            Format = "Move {0}",
            GetPrimaryTarget = t => t.GetTarget(TargetIndex.A)
        };

        static public TaskDef Harvesting = new("Harvesting", typeof(TaskBehaviorHarvestingNew))
        {
            Format = "Harvest {0}",
            GetPrimaryTarget = t => t.GetTarget(TargetIndex.A)
        };

        static public TaskDef Digging = new("Digging", typeof(TaskBehaviorDiggingNewNew))
        {
            Format = "Dig {0}",
            GetPrimaryTarget = t => t.GetTarget(TargetIndex.A)
        };

        static public TaskDef SleepingOnGround = new("SleepingOnGround", typeof(TaskBehaviorSleepOnGround))
        {
            Format = "Sleep on ground",
            GetPrimaryTarget = t => TargetArgs.Null
        };

        static public TaskDef Chatting = new("Chatting", typeof(TaskBehaviorTalkToAboutTopic))
        {
            Format = "Chat",
            GetPrimaryTarget = t => t.GetTarget(TargetIndex.A)
        };
    }
}
