namespace Start_a_Town_
{
    partial class ItemPreferencesManager
    {
        class ItemPreferenceProperties
        {
            public Entity Item;
            public bool Keep;
            public JobDef Job;
            public GearType Gear;
            public ToolAbilityDef ToolUse;
            public ItemPreferenceProperties(Entity item)
            {
                this.Item = item;
            }
        }
    }
}
