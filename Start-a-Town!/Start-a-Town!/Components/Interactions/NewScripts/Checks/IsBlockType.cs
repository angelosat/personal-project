using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components;

namespace Start_a_Town_
{
    public class IsBlockType : ScriptTaskCondition
    {
        public override string Name
        {
            get
            {
                return "IsBlockType: " + this.Type.ToString();
            }
        }
        Block.Types Type;

        public IsBlockType(Block.Types type)
        {
            this.Type = type;
            this.ErrorEvent = Message.Types.InvalidTargetType;
        }
        public override bool Condition(GameObject actor, TargetArgs target)
        {
            if (target.Type != TargetType.Position)
                throw new Exception();
            var block = actor.Map.GetBlock(target.Global);
            return block.Type == this.Type;
        }

        public override ScriptTaskCondition GetFailedCondition(GameObject actor, TargetArgs target)
        {
            if (!this.Condition(actor, target))
                return this;
            return null;
        }
    }
}
