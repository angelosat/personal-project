using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_.Components.Interactions
{
    class ProcessMaterial : Interaction
    {
        //Skill Skill;

        public ProcessMaterial(string name, ToolAbilityDef skill)
            : base(
                name,
                2
                )
        {

            this.Skill = skill;
            //this.Conditions = new TaskConditions(
            //        new AllCheck(
            //            new TargetTypeCheck(TargetType.Entity),
            //            new SkillCheck(skill),
            //            new RangeCheck(t => t.Global, InteractionOld.DefaultRange),
            //            new ScriptTaskCondition("IsRawMaterial", (a, t) => t.Object.HasComponent<RawMaterialComponent>()),
            //            new ScriptTaskCondition("ValidMaterial", (a, t) => t.Object.GetComponent<RawMaterialComponent>().SkillToProcess == skill))
            //    );
        }

        public override bool AvailabilityCondition(GameObject actor, TargetArgs target)
        {
            return this.Conditions.GetFailedCondition(actor, target) == null;
            //return this.Conditions(actor, target);
            //return new SkillCheck(this.Skill).Condition(actor, target);
        }

        public override void Perform(GameObject a, TargetArgs t)
        {
            // TODO: destroy target here and spawn extracted raw materials
            RawMaterialComponent comp = t.Object.GetComponent<RawMaterialComponent>();
            comp.Process(t.Object, a);
        }
        public override object Clone()
        {
            return new ProcessMaterial(this.Name, this.Skill);
        }
    }
}
