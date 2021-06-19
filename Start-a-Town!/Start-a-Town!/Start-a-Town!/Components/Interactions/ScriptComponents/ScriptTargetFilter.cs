using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_.Components.Interactions
{
    class ScriptTargetFilter : ScriptComponent
    {
        public override string ComponentName
        {
            get { return "ScriptTargetFilter"; }
        }
        public override object Clone()
        {
            return new ScriptTargetFilter(this.Condition);
        }

        Func<GameObject, bool> Condition;
        public ScriptTargetFilter(Func<GameObject, bool> condition)
        {
            this.Condition = condition;
        }

        public override bool Evaluate(ScriptArgs args)
        {
            //return this.Condition(args.Target.Object);
            if (this.Condition(args.Target.Object))
                return true;
            args.Net.EventOccured(Message.Types.InvalidTarget, args.Actor);
            return false;
        }
    }
}
