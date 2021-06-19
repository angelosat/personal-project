using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Start_a_Town_.Components.Skills;
using Start_a_Town_.Components.Interactions;

namespace Start_a_Town_.Components
{
    class SkillComponentNew : EntityComponent
    {
        public override string ComponentName
        {
            get { return "SkillComponentNew"; }
        }
        public override object Clone()
        {
            return new SkillComponentNew().Initialize(this.Skills);
        }

        List<ToolAbilityDef> Skills { get; set; }
        public SkillComponentNew()
        {

        }
        public SkillComponentNew Initialize(IEnumerable<ToolAbilityDef> skills)
        {
            this.Skills = new List<ToolAbilityDef>(skills);
            return this;
        }
        public SkillComponentNew Initialize(params ToolAbilityDef[] skills)
        {
            this.Skills = new List<ToolAbilityDef>(skills);
            return this;
        }

        public ToolAbilityDef Skill { get { return this.Skills.FirstOrDefault(); } }

        static public bool HasSkill(GameObject obj, ToolAbilityDef skill)
        {
            if (obj == null)
                return false;
            SkillComponentNew comp;
            if (!obj.TryGetComponent<SkillComponentNew>(out comp))
                return false;
            return comp.Skills.Contains(skill);
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
            // TODO: put getinteractionsfromskill in targetargs class and handle case where target is a block
            if (t.Type != TargetType.Entity)
                return;
            var actions = t.Object.GetInteractionsFromSkill(this.Skill);
            list.AddRange(actions);
        }

        //public override void GetEquippedActions(GameObject parent, List<Interactions.Interaction> actions)
        //{
        //    actions.Add(this.Skill.GetWork());
        //}

        //public override void Write(BinaryWriter w)
        //{
        //    w.Write(this.Skills.Count);
        //    foreach (var item in this.Skills)
        //        w.Write(item.ID);
        //}
        //public override void Read(BinaryReader r)
        //{
        //    int count = r.ReadInt32();
        //    for (int i = 0; i < count; i++)
        //    {
        //        this.Skills.Add(Skill.Dictionary[r.ReadInt32()]);
        //    }
        //}
        //internal override List<SaveTag> Save()
        //{
        //    SaveTag list = new SaveTag(SaveTag.Types.List, "SkillIDs", SaveTag.Types.Int);
        //    foreach (var item in this.Skills)
        //        list.Add(new SaveTag(SaveTag.Types.Int, "", item.ID));
        //     return new List<SaveTag>() {list};
        //}
        //internal override void Load(SaveTag save)
        //{
        //    //foreach (var item in (List<SaveTag>)save.Value)
        //    //    if (item.Value != null)
        //    //        this.Skills.Add(Skill.Dictionary[(int)item.Value]);
        //}
    }
}
