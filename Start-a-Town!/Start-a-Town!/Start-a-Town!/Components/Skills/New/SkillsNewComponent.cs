using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_.Components.Skills.New
{
    class SkillsNewComponent : Component
    {
        public override string ComponentName
        {
            get { return "SkillsNew"; }
        }

        public List<Skill> SkillList = new List<Skill>();

        public SkillsNewComponent()
        {
        }
        public SkillsNewComponent(IEnumerable<Skill> skills)
        {
            this.SkillList = new List<Skill>(skills);
        }
        public SkillsNewComponent(params Skill[] skills)
        {
            this.SkillList = new List<Skill>(skills);
        }
        public override string ToString()
        {
            var t = "";
            foreach (var sk in this.SkillList)
                t += sk.ToString() + '\n';
            return t.TrimEnd('\n');
        }
        public override object Clone()
        {
            var newskills = (from skill in this.SkillList select skill.Clone() as Skill).ToList();
            return new SkillsNewComponent(newskills);
        }
    }
}
