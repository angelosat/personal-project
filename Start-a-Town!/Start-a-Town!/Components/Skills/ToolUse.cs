namespace Start_a_Town_
{
    public struct ToolUse
    {
        public readonly ToolUseDef Def;
        public int Effectiveness;

        public ToolUse(ToolUseDef def, int efficiency)
        {
            this.Def = def;
            this.Effectiveness = 1;// efficiency;
        }
    }
}
