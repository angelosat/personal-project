using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.Components.Interactions;
using Start_a_Town_.Modules.Construction;

namespace Start_a_Town_.Components.Skills
{
    /*
    static public readonly Skill Building = new Skill("Building", "Build blocks and other structures", (t) =>
            {
                if (t.Object.HasComponent<StructureComponent>())
                    return new BuildStructure();
                else if (t.Object.HasComponent<ConstructionComponent>())
                    return new BuildBlock();
                return null;
            });
*/
    class SkillBuilding : Skill
    {
        //public SkillBuilding()
        //{
        //    this.Name = "Building";
        //    this.Description = "Build blocks and other structures";
        //}
        public SkillBuilding()
            : base("Building", "Build blocks and other structures")
        {

        }

        public override Interactions.Interaction GetWork(GameObject a, TargetArgs t)
        {
            //if (t.Object.HasComponent<StructureComponent>())
            //    return new BuildStructure();
            //else if (t.Object.HasComponent<ConstructionComponent>())
            //    return new BuildBlock();
            return null;
        }
    }
}
