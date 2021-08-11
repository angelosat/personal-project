namespace Start_a_Town_
{
    static class ToolPropsDefof
    {
        public static readonly ToolProps Shovel = new("Shovel")
        {
            Description = "Used to dig out grainy material like soil dirt and sand.",
            SpriteHandle = ItemContent.ShovelHandle,
            SpriteHead = ItemContent.ShovelHead,
            Ability = new ToolUse(ToolUseDef.Digging, 5),
            AssociatedJobs = new() { JobDefOf.Digger }
        };
        public static readonly ToolProps Axe = new("Axe")
        {
            Description = "Chops down trees.",
            SpriteHandle = ItemContent.AxeHandle,
            SpriteHead = ItemContent.AxeHead,
            Ability = new ToolUse(ToolUseDef.Chopping, 5),
            AssociatedJobs = new() { JobDefOf.Lumberjack }
        };
        public static readonly ToolProps Hammer = new("Hammer")
        {
            Description = "Used for building.",
            SpriteHandle = ItemContent.HammerHandle,
            SpriteHead = ItemContent.HammerHead,
            Ability = new ToolUse(ToolUseDef.Building, 5),
            AssociatedJobs = new() { JobDefOf.Builder }
        };
        public static readonly ToolProps Pickaxe = new("Pickaxe")
        {
            Description = "Used for mining.",
            SpriteHandle = ItemContent.PickaxeHandle,
            SpriteHead = ItemContent.PickaxeHead,
            Ability = new ToolUse(ToolUseDef.Mining, 5),
            AssociatedJobs = new() { JobDefOf.Miner }
        };
        public static readonly ToolProps Handsaw = new("Handsaw")
        {
            Description = "Used for carpentry.",
            SpriteHandle = ItemContent.HandsawHandle,
            SpriteHead = ItemContent.HandsawHead,
            Ability = new ToolUse(ToolUseDef.Carpentry, 5),
            AssociatedJobs = new() { JobDefOf.Carpenter }
        };
        public static readonly ToolProps Hoe = new("Hoe")
        {
            Description = "Used to prepare soil for planting by converting it into farmland.",
            SpriteHandle = ItemContent.HoeHandle,
            SpriteHead = ItemContent.HoeHead,
            Ability = new ToolUse(ToolUseDef.Argiculture, 5),
            AssociatedJobs = new() { JobDefOf.Farmer }
        };
    }
}
