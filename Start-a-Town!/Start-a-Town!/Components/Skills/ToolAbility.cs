namespace Start_a_Town_
{
    public struct ToolAbility
    {
        public readonly ToolAbilityDef Def;
        public int Effectiveness;

        public ToolAbility(ToolAbilityDef def, int efficiency)
        {
            this.Def = def;
            this.Effectiveness = efficiency;
        }
    }
}
