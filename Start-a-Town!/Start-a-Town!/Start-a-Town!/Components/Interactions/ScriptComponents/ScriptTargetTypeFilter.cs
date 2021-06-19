using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_.Components.Interactions
{
    class ScriptTargetTypeFilter : ScriptComponent
    {
        public override string ComponentName
        {
            get { return "ScriptTargetTypeFilter"; }
        }
        public override object Clone()
        {
            return new ScriptTargetTypeFilter(this.Condition);
        }

        Func<TargetArgs, bool> Condition;
        public ScriptTargetTypeFilter(Func<TargetArgs, bool> condition)
        {
            this.Condition = condition;
        }

        public override bool Evaluate(ScriptArgs args)
        {
            if (this.Condition(args.Target))
                return true;
            args.Net.EventOccured(Message.Types.InvalidTargetType, args.Actor);
            return false;
        }
    }
}
