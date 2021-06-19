using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components;

namespace Start_a_Town_
{
    public class BlockCheck : ScriptTaskCondition
    {
        Func<Block, bool> Evaluation;

        public BlockCheck(Func<Block, bool> evaluation)
            : base("Blockcheck")
        {
            this.Evaluation = evaluation;
            this.ErrorEvent = Message.Types.InvalidTargetType;
        }
        public override bool Condition(GameObject actor, TargetArgs target)
        {
            if (target.Type != TargetType.Position)
                throw new Exception();
            var block = actor.Map.GetBlock(target.Global);
            return this.Evaluation(block);
        }

        public override ScriptTaskCondition GetFailedCondition(GameObject actor, TargetArgs target)
        {
            if (!this.Condition(actor, target))
                return this;
            return null;
        }
    }
}
