using System.Collections.Generic;
using System.Linq;
using Start_a_Town_.UI;
using Start_a_Town_.Components;
using System.IO;

namespace Start_a_Town_
{
    class ToolAbilityComponent : EntityComponent
    {
        readonly static public string Name = "Tool";
        public override string ComponentName => Name;
        public override object Clone()
        {
            return new ToolAbilityComponent(this.Props);
        }
        public ToolProps Props;
        readonly List<ToolAbilityDef> Skills = new();
        readonly Dictionary<int, float> WorkCapabilities = new();
        public ToolAbilityComponent(ToolProps props)
        {
            this.Props = props;
        }
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
        public override void OnObjectLoaded(GameObject parent)
        {
            base.OnObjectLoaded(parent);
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

        public override void Write(BinaryWriter w)
        {
            w.Write(this.Props is not null); // HACK for loading legacy items which lack Props
            this.Props?.Write(w);
        }
        public override void Read(BinaryReader r)
        {
            if (r.ReadBoolean()) // HACK for loading legacy items which lack Props
                this.Props = Def.GetDef<ToolProps>(r);
        }
    }
}
