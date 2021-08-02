using System.Linq;

namespace Start_a_Town_
{
    class TaskGiverEquipSelf : TaskGiver
    {
        protected override AITask TryAssignTask(Actor actor)
        {
            // TODO associate labors with tool, if labor is enabled, look for and store tools in inventory. if labor is disabled, remove unnecessary tools from inventory
            // TODO flag jobs for which a tool is already aquired so as to not recheck everything all the time
            if (!actor.IsCitizen)
                return null; // TODO instead of doing this, check if the tool is claimable
            if (DropUnnecessaryItems(actor) is AITask task)
                return task;
            var jobs = actor.GetJobs();
            foreach (var job in jobs)
            {
                var itemmanager = actor.ItemPreferences;
                var toolUse = job.Def.ToolUse;
                if (toolUse is null)
                    continue;
                var preferredTool = itemmanager.GetPreference(toolUse, out var existingScore);

                // see if there are better tools lying around
                if (job.Enabled)
                {
                    var potentialTools = actor.Map.Find(i => i.ToolComponent?.Props?.Ability.Def == toolUse);
                    var scoredTools = potentialTools.Select(i => new { item = i, score = itemmanager.GetScore(toolUse, i) }).OrderByDescending(i => i.score);
                    foreach (var tool in scoredTools)
                    {
                        if (tool.score <= existingScore)
                            break;
                        if (!actor.CanReserve(tool.item))
                            continue;
                        if (!actor.CanReach(tool.item))
                            continue;
                        itemmanager.AddPreference(toolUse, tool.item, tool.score);
                        return new AITask(typeof(TaskBehaviorStoreInInventory)) { TargetA = tool.item, AmountA = 1 };
                    }
                }

                // otherwise, if there are no better tools to go pick up, do actions with the existing preferred tool
                if (preferredTool is not null)
                {
                    if (!actor.Inventory.Contains(preferredTool))
                    {
                        // if it's not inside inventory, instead of going to pick it up...
                        // ...remove preference and check for a tool next tick (it might be a leftover from a previous failed behavior, or the item might no longer be available)
                        itemmanager.RemovePreference(toolUse);
                    }
                    else
                    {
                        if (!job.Enabled)
                        {
                            itemmanager.RemovePreference(toolUse);
                            return new AITask(typeof(TaskBehaviorDropItem), preferredTool);
                        }
                    }
                }
            }
            return null;
        }
        static AITask DropUnnecessaryItems(Actor actor)
        {
            if (actor.Inventory.All.FirstOrDefault(i => !actor.ItemPreferences.IsPreference(i)) is Entity item)
                return new AITask(typeof(TaskBehaviorDropInventoryItem), item);
            return null;
        }
    }
}
