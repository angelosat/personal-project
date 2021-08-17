namespace Start_a_Town_
{
    [EnsureStaticCtorCall]
    static class BuildToolDefOf
    {
        public static readonly BuildToolDef Single = new("Single", typeof(ToolBuildSingle), typeof(BuildToolWorkerSingle));
        public static readonly BuildToolDef SinglePreview = new("SinglePreview", typeof(ToolBuildSinglePreview), typeof(BuildToolWorkerSingle));
        public static readonly BuildToolDef Line = new("Line", typeof(ToolBuildLine), typeof(BuildToolWorkerLine));
        public static readonly BuildToolDef Floor = new("Floor", typeof(ToolBuildFloor), typeof(BuildToolWorkerFloor));
        public static readonly BuildToolDef Wall = new("Wall", typeof(ToolBuildWall), typeof(BuildToolWorkerWall));
        public static readonly BuildToolDef Enclosure = new("Enclosure", typeof(ToolBuildEnclosure), typeof(BuildToolWorkerEnclosure));
        public static readonly BuildToolDef Box = new("Box", typeof(ToolBuildBox), typeof(BuildToolWorkerBox));
        public static readonly BuildToolDef BoxFilled = new("BoxFilled", typeof(ToolBuildBoxFilled), typeof(BuildToolWorkerBoxFilled));
        public static readonly BuildToolDef Roof = new("Roof", typeof(ToolBuildRoof), typeof(BuildToolWorkerRoof));
        public static readonly BuildToolDef Pyramid = new("Pyramid", typeof(ToolBuildPyramid), typeof(BuildToolWorkerPyramid));
        static BuildToolDefOf()
        {
            Def.Register(typeof(BuildToolDefOf));
        }
    }
}
