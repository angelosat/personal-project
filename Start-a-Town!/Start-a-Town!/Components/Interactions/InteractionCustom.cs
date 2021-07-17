using System;

namespace Start_a_Town_.Components.Interactions
{
    class InteractionCustom : Interaction
    {
        public InteractionCustom(string name, Action<GameObject, TargetArgs> callback)
            :base(name, callback)
        {
        }
        public InteractionCustom(string name, float seconds, Action<GameObject, TargetArgs> callback, ToolAbilityDef skill)
            :base(name, seconds, callback, skill)
        {
        }
        
        public InteractionCustom(string name, float seconds, Action<GameObject, TargetArgs> action)
            : base(name, seconds, action)
        {

        }
       
        public override object Clone()
        {
            return new InteractionCustom(this.Name, this.Seconds, this.Callback, this.Skill);
        }
    }
}
