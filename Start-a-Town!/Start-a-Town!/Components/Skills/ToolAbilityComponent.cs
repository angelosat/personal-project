using System.Collections.Generic;
using System.Linq;
using Start_a_Town_.UI;

namespace Start_a_Town_.Components
{
    class ToolAbilityComponent : EntityComponent
    {
        readonly static public string Name = "Tool";
        public override string ComponentName
        {
            get { return Name; }
        }
        public override object Clone()
        {
            return new ToolAbilityComponent();
        }

        readonly List<ToolAbilityDef> Skills = new();
        readonly Dictionary<int, float> WorkCapabilities = new();
        public ToolAbilityComponent()
        {

        }
        
        public ToolAbilityComponent(params ToolAbilityDef[] skills)
        {

        }
        public ToolAbilityComponent Initialize(params ToolAbilityDef[] skills)
        {
            return this;
        }
       
        public ToolAbilityDef Skill { get { return this.Skills.FirstOrDefault(); } }
        
        static public bool HasSkill(GameObject parent, ToolAbilityDef skill)
        {
            return parent.Def.ToolProperties?.Ability.Def == skill || HasSkill(parent, skill.ID);
        }
        static public bool HasSkill(GameObject parent, int skillID)
        {
            return 
                parent.Def.ToolProperties?.Ability.Def.ID == skillID || 
                (parent.GetComponent<ToolAbilityComponent>()?.WorkCapabilities.ContainsKey(skillID) ?? false);
        }
        public override string ToString()
        {
            if (this.Skills.Count == 0)
                return "";
            string text = "";
            foreach (var item in this.Skills)
                text += item.Name + "\n";
            return text.TrimEnd('\n');
        }
        public override void GetEquippedActions(GameObject parent, List<Interaction> actions)
        {
            actions.Add(this.Skill.GetInteraction());
        }

        internal override void GetEquippedActionsWithTarget(GameObject gameObject, GameObject actor, TargetArgs t, List<Interaction> list)
        {
            list.Add(this.Skill.GetInteraction());
        }
       
        public override void OnTooltipCreated(GameObject parent, Control tooltip)
        {
            tooltip.AddControlsBottomLeft(this.GetUI(parent));
        }
        GroupBox GetUI(GameObject parent)
        {
            var box = new GroupBox();
            foreach(var s in this.WorkCapabilities)
            {
                box.AddControlsBottomLeft(ToolAbilityDef.GetUI(s.Key, s.Value));
            }
            var ability = parent.Def.ToolProperties?.Ability;

            if (ability != null)
                box.AddControlsBottomLeft(ToolAbilityDef.GetUI(ability.Value.Def.ID, ability.Value.Efficiency));
            return box;
        }
    }
}
