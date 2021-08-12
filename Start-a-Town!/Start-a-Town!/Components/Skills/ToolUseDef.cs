namespace Start_a_Town_
{
    public class ToolUseDef : Def, IItemPreferenceContext
    {
        public string Description { get; protected set; }

        public ToolUseDef(string name, string description) : base(name)
        {
            this.Description = description;
        }

        public override string ToString()
        {
            return this.Name;
        }
    }

    static class ToolUseDefOf
    {
        public static readonly ToolUseDef Digging = new("Digging", "Dig up soil and dirt blocks");
        public static readonly ToolUseDef Building = new("Building", "Build blocks and other structures");
        public static readonly ToolUseDef Mining = new("Mining", "Dig up stone blocks");
        public static readonly ToolUseDef Chopping = new("Chopping", "Chop down trees and enemies with axes");
        public static readonly ToolUseDef Argiculture = new("Argiculture", "Helps determine type and growth time of plants.");
        public static readonly ToolUseDef Planting = new("Planting", "Planting plants");
        public static readonly ToolUseDef Carpentry = new("Carpentry", "The craft of converting wood to useful equipment");
        static ToolUseDefOf()
        {
            Def.Register(Digging);
            Def.Register(Building);
            Def.Register(Mining);
            Def.Register(Chopping);
            Def.Register(Argiculture);
            Def.Register(Planting);
            Def.Register(Carpentry);
        }
    }
}
