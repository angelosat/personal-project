using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components.Materials;

namespace Start_a_Town_.Components.Interactions
{
    public class BlockMaterialCheck : ScriptTaskCondition
    {
        Func<Material, bool> Evaluation;

        public BlockMaterialCheck(Func<Material, bool> evaluation)
            : base("BlockMaterialCheck")
        {
            this.Evaluation = evaluation;
            this.ErrorEvent = Message.Types.InvalidTargetType;
        }
        public override bool Condition(GameObject actor, TargetArgs target)
        {
            if (target.Type != TargetType.Position)
                throw new Exception();
            var mat = Block.GetBlockMaterial(actor.Map, target.Global);
            return this.Evaluation(mat);
        }

        public override ScriptTaskCondition GetFailedCondition(GameObject actor, TargetArgs target)
        {
            if (!this.Condition(actor, target))
                return this;
            return null;
        }
    }
}
