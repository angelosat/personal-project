using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.Components.Interactions
{
    public class TargetTypeCheck : ScriptTaskCondition
    {
        TargetType Type { get; set; }
        float Min, Max;

        public TargetTypeCheck(TargetType type)
            : base("TargetType")
        {
            this.Type = type;
            this.ErrorEvent = Message.Types.InvalidTargetType;
        }
        public override bool Condition(GameObject actor, TargetArgs target)
        {
            return target.Type == this.Type;
        }

        public override ScriptTaskCondition GetFailedCondition(GameObject actor, TargetArgs target)
        {
            if (!this.Condition(actor, target))
                return this;
            return null;
        }
    }
}
