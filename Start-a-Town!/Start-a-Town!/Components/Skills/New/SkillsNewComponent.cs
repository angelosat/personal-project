using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_.Components.Skills.New
{
    class SkillsNewComponent : EntityComponent
    {
        public override string ComponentName
        {
            get { return "SkillsNew"; }
        }

        public List<SkillNew> SkillList = new List<SkillNew>();

        public SkillsNewComponent()
        {
        }
        public SkillsNewComponent(IEnumerable<SkillNew> skills)
        {
            this.SkillList = new List<SkillNew>(skills);
        }
        public SkillsNewComponent(params SkillNew[] skills)
        {
            this.SkillList = new List<SkillNew>(skills);
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
            var newskills = (from skill in this.SkillList select skill.Clone() as SkillNew).ToList();
            return new SkillsNewComponent(newskills);
        }
    }
}
