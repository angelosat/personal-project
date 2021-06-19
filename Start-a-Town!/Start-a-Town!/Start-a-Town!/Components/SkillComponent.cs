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
    class SkillComponent : Component
    {
        readonly static public string Name = "Tool";
        public override string ComponentName
        {
            get { return Name; }// "SkillComponent"; }
        }
        public override object Clone()
        {
            return new SkillComponent().Initialize(this.Skills);
        }

        List<Skill> Skills { get; set; }
        public SkillComponent()
        {

        }
        public SkillComponent Initialize(IEnumerable<Skill> skills)
        {
            this.Skills = new List<Skill>(skills);
            return this;
        }
        public SkillComponent Initialize(params Skill[] skills)
        {
            this.Skills = new List<Skill>(skills);
            return this;
        }

        public Skill Skill { get { return this.Skills.FirstOrDefault(); } }

        static public bool HasSkill(GameObject obj, Skill skill)
        {
            if (obj == null)
                return false;
            SkillComponent comp;
            if (!obj.TryGetComponent<SkillComponent>(out comp))
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
        public override void GetEquippedActions(GameObject parent, List<Interactions.Interaction> actions)
        {
            actions.Add(this.Skill.GetWork());
        }

        internal override void GetEquippedActionsWithTarget(GameObject gameObject, GameObject actor, TargetArgs t, List<Interaction> list)
        {
            list.Add(this.Skill.GetWork());
        }

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
