using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Start_a_Town_.Components.Skills;
using Start_a_Town_.Components.Interactions;
using Start_a_Town_.UI;

namespace Start_a_Town_.Components
{
    class ToolAbilityComponent : EntityComponent
    {
        readonly static public string Name = "Tool";
        public override string ComponentName
        {
            get { return Name; }// "SkillComponent"; }
        }
        public override object Clone()
        {
            return new ToolAbilityComponent();//.Initialize(this.WorkCapabilities);
        }



        readonly List<ToolAbilityDef> Skills = new();//; { get; set; }
        readonly Dictionary<int, float> WorkCapabilities = new();
        public ToolAbilityComponent()
        {

        }
        //public ToolAbilityComponent Initialize(IEnumerable<ToolAbility> skills)
        //{
        //    this.Skills = new List<ToolAbility>(skills);
        //    foreach (var s in skills)
        //        this.AddSkill(s.ID);
        //    return this;
        //}
        public ToolAbilityComponent(params ToolAbilityDef[] skills)
        {

        }
        public ToolAbilityComponent Initialize(params ToolAbilityDef[] skills)
        {
            //this.Skills = new List<ToolAbility>(skills);
            //foreach (var s in skills)
            //    this.AddSkill(s.ID);
            return this;
        }
        //private object Initialize(Dictionary<int, float> dictionary)
        //{
        //    foreach (var i in dictionary)
        //        this.AddSkill(i.Key, i.Value);
        //    return this;
        //}

        public ToolAbilityDef Skill { get { return this.Skills.FirstOrDefault(); } }
        //public ToolAbilityComponent AddSkill(ToolAbility skill, float work = 1)
        //{
        //    return this.AddSkill(skill.ID, work);
        //}
        //public ToolAbilityComponent AddSkill(int skill, float work = 1)
        //{
        //    this.WorkCapabilities[skill] = work;
        //    return this;
        //}
        static public bool HasSkill(GameObject parent, ToolAbilityDef skill)
        {
            return parent.Def.ToolProperties?.Ability.Def == skill || HasSkill(parent, skill.ID);
            //return HasSkill(parent, skill.ID);

        }
        static public bool HasSkill(GameObject parent, int skillID)
        {
            return 
                parent.Def.ToolProperties?.Ability.Def.ID == skillID || 
                (parent.GetComponent<ToolAbilityComponent>()?.WorkCapabilities.ContainsKey(skillID) ?? false);

            if (parent == null)
                return false;
            ToolAbilityComponent comp;
            if (!parent.TryGetComponent<ToolAbilityComponent>(out comp))
                return false;
            return comp.WorkCapabilities.ContainsKey(skillID);
            //return comp.Skills.Any(s => s.ID == skillID);
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
        //static public float GetWorkCapability(GameObject obj, int skillid)
        //{
        //    //var ability = obj.GetDef<ItemToolDef>()?.Ability;
        //    var ability = obj.Def.ToolProperties?.Ability;

        //    if (!ability.HasValue)
        //        throw new Exception();
        //    return ability.Value.Efficiency;
        //    //ToolAbilityComponent comp;
        //    //obj.TryGetComponent<ToolAbilityComponent>(out comp);
        //    //return comp.WorkCapabilities[skillid];
        //}
        
        public override void OnTooltipCreated(GameObject parent, Control tooltip)
        {
            tooltip.AddControlsBottomLeft(this.GetUI(parent));
        }
        GroupBox GetUI(GameObject parent)
        {
            var box = new GroupBox();
            foreach(var s in this.WorkCapabilities)
            {
                box.AddControlsBottomLeft(ToolAbilityDef.GetUI(s.Key, s.Value));// new Label(Skill.GetSkill(s.Key).ToString()));
            }
            //var ability = parent.GetDef<ItemToolDef>()?.Ability;
            var ability = parent.Def.ToolProperties?.Ability;

            if (ability != null)
                box.AddControlsBottomLeft(ToolAbilityDef.GetUI(ability.Value.Def.ID, ability.Value.Efficiency));// new Label(Skill.GetSkill(s.Key).ToString()));

            return box;
        }
        
    }
}
