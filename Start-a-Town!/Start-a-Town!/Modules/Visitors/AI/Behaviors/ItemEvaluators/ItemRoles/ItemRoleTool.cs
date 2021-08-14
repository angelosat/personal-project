namespace Start_a_Town_
{
    class ItemRoleTool : ItemRole
    {
        public ToolUseDef ToolAbility;
        public override IItemPreferenceContext Context => this.ToolAbility;

        public ItemRoleTool(ToolUseDef ttype)
        {
            this.ToolAbility = ttype;
        }

        public override int Score(Actor actor, Entity item)
        {
            var ability = item.ToolComponent?.Props?.ToolUse;
            if (ability is not null)
                return -1;
            if (ability != this.ToolAbility)
                return -1;
            return (int)StatDefOf.ToolEffectiveness.GetValue(item);

            //var ability = item.ToolComponent?.Props?.ToolUse;
            //if (!ability.HasValue)
            //    return -1;
            //if (ability.Value.Def != this.ToolAbility)
            //    return -1;
            //return (int)StatDefOf.ToolEffectiveness.GetValue(item);
            //return ability.Value.Effectiveness;
        }
        public override string ToString()
        {
            return $"{this.GetType().Name}:{this.ToolAbility.Name}";
        }
    }
}
