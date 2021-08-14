namespace Start_a_Town_
{
    [EnsureStaticCtorCall]
    static class ToolPropsDefof
    {
        public static readonly ToolProps Shovel = new("Shovel")
        {
            Description = "Used to dig out grainy material like soil dirt and sand.",
            SpriteHandle = ItemContent.ShovelHandle,
            SpriteHead = ItemContent.ShovelHead,
            //ToolUse = new ToolUse(ToolUseDefOf.Digging, 5),
            ToolUse = ToolUseDefOf.Digging,
            Skill = SkillDefOf.Digging,
            AssociatedJobs = new() { JobDefOf.Digger }
        };
        public static readonly ToolProps Axe = new("Axe")
        {
            Description = "Chops down trees.",
            SpriteHandle = ItemContent.AxeHandle,
            SpriteHead = ItemContent.AxeHead,
            //ToolUse = new ToolUse(ToolUseDefOf.Chopping, 5),
            ToolUse = ToolUseDefOf.Chopping,
            Skill = SkillDefOf.Plantcutting,
            AssociatedJobs = new() { JobDefOf.Lumberjack }
        };
        public static readonly ToolProps Hammer = new("Hammer")
        {
            Description = "Used for building.",
            SpriteHandle = ItemContent.HammerHandle,
            SpriteHead = ItemContent.HammerHead,
            ToolUse = ToolUseDefOf.Building,
            //ToolUse = new ToolUse(ToolUseDefOf.Building, 5),
            Skill = SkillDefOf.Construction,
            AssociatedJobs = new() { JobDefOf.Builder }
        };
        public static readonly ToolProps Pickaxe = new("Pickaxe")
        {
            Description = "Used for mining.",
            SpriteHandle = ItemContent.PickaxeHandle,
            SpriteHead = ItemContent.PickaxeHead,
            ToolUse = ToolUseDefOf.Mining,
            //ToolUse = new ToolUse(ToolUseDefOf.Mining, 5),
            Skill = SkillDefOf.Mining,
            AssociatedJobs = new() { JobDefOf.Miner }
        };
        public static readonly ToolProps Handsaw = new("Handsaw")
        {
            Description = "Used for carpentry.",
            SpriteHandle = ItemContent.HandsawHandle,
            SpriteHead = ItemContent.HandsawHead,
            ToolUse = ToolUseDefOf.Carpentry,
            //ToolUse = new ToolUse(ToolUseDefOf.Carpentry, 5),
            Skill = SkillDefOf.Carpentry,
            AssociatedJobs = new() { JobDefOf.Carpenter }
        };
        public static readonly ToolProps Hoe = new("Hoe")
        {
            Description = "Used to prepare soil for planting by converting it into farmland.",
            SpriteHandle = ItemContent.HoeHandle,
            SpriteHead = ItemContent.HoeHead,
            ToolUse = ToolUseDefOf.Argiculture,
            //ToolUse = new ToolUse(ToolUseDefOf.Argiculture, 5),
            Skill = SkillDefOf.Argiculture,
            AssociatedJobs = new() { JobDefOf.Farmer }
        };
        static ToolPropsDefof()
        {
            Def.Register(typeof(ToolPropsDefof));
        }
        public static void Init() { }
    }
}
