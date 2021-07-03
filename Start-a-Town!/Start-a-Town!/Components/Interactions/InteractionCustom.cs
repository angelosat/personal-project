using System;

namespace Start_a_Town_.Components.Interactions
{
    class InteractionCustom : Interaction
    {
        readonly TaskConditions CustomConditions = new TaskConditions();

        public InteractionCustom(string name, Action<GameObject, TargetArgs> callback)
            :base(name, callback)
        {
        }
        public InteractionCustom(string name, float seconds, Action<GameObject, TargetArgs> callback, TaskConditions conditions, ToolAbilityDef skill)
            :base(name, seconds, callback, conditions, skill)
        {
        }
        public override TaskConditions Conditions
        {
            get
            {
                return this.CustomConditions;
            }
        }
        public InteractionCustom(string name, float seconds, Action<GameObject, TargetArgs> action)
            : base(name, seconds, action)
        {

        }
        public InteractionCustom(string name, float seconds, Action<GameObject, TargetArgs> action, TaskConditions conds)
            :base(name, seconds, action)
        {
            this.CustomConditions = conds;
        }
        public override object Clone()
        {
            return new InteractionCustom(this.Name, this.Seconds, this.Callback, this.Conditions, this.Skill);
        }
    }
}
