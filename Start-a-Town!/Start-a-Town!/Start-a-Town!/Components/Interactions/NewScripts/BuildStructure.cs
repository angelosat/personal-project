using System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.Components;
using Start_a_Town_.Components.Skills;

namespace Start_a_Town_.Components.Interactions
{
    class BuildStructure : Interaction
    {
        //public static readonly ScriptTask BuildBlock = new ScriptTask("Building", 2, actor => this.Finish(parent, actor), new TaskConditionCollection(
        //        new RangeCondition(() => parent.Global, Interaction.DefaultRange),
        //        new ScriptTaskCondition("Materials", actor => this.DetectMaterials(parent, actor), Message.Types.InsufficientMaterials)

        //        ));
        public BuildStructure()
            : base(
            "Building",
            2,
            (a, t) => t.Object.GetComponent<StructureComponent>().Finish(t.Object, a),
            new TaskConditions(new AllCheck(
                new RangeCheck(t => t.Object.Global, InteractionOld.DefaultRange),
                new ScriptTaskCondition("Materials", (a, t) => t.Object.GetComponent<StructureComponent>().DetectMaterials(t.Object, a), Message.Types.InsufficientMaterials),
                new SkillCheck(Skill.Building))))
        { }

        public override object Clone()
        {
            return new BuildStructure();
        }
    }
}
