using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.Components;

namespace Start_a_Town_.Components.Interactions
{
    class BuildBlock : Interaction
    {
        //public static readonly ScriptTask BuildBlock = new ScriptTask("Building", 2, actor => this.Finish(parent, actor), new TaskConditionCollection(
        //        new RangeCondition(() => parent.Global, Interaction.DefaultRange),
        //        new ScriptTaskCondition("Materials", actor => this.DetectMaterials(parent, actor), Message.Types.InsufficientMaterials)

        //        ));
        public BuildBlock()
            : base(
            "Building", 
            2,
            (a, t) => t.Object.GetComponent<ConstructionComponent>().Finish(t.Object, a),
            new TaskConditions(
                new AllCheck(
                new RangeCheck(t => t.Object.Global, InteractionOld.DefaultRange),
                //new ScriptTaskCondition("Materials", (a, t) => t.Object.GetComponent<ConstructionComponent>().DetectMaterials(t.Object, a), Message.Types.InsufficientMaterials)))
                new ScriptTaskCondition("Materials", (a, t) => t.Object.GetComponent<ConstructionComponent>().BuildCondition(), Message.Types.InsufficientMaterials))))
        { }

        public override bool AvailabilityCondition(GameObject a, TargetArgs t)
        {
            //if(GearComponent.GetSlot(a, GearType.Mainhand))
            var toolSlot = GearComponent.GetSlot(a, GearType.Mainhand);
            if (toolSlot.Object == null)
                return false;
            if(SkillComponent.HasSkill(toolSlot.Object, Skills.Skill.Building))
            return t.Object.GetComponent<ConstructionComponent>().BuildCondition();
            return false;
        }

        public override object Clone()
        {
            return new BuildBlock();
        }
    }
}
