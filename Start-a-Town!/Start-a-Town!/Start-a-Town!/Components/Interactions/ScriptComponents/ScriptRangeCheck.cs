using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_.Components.Interactions
{
    class ScriptRangeCheck : ScriptComponent
    {
        float Max, Min;
        public override string ComponentName
        {
            get { return "ScriptRangeCheck"; }
        }
        public override object Clone()
        {
            return new ScriptRangeCheck(this.Condition);
        }

        Func<ScriptArgs, bool> Condition;
        public ScriptRangeCheck(Func<ScriptArgs, bool> condition)
        {
            this.Condition = condition;
        }
        public ScriptRangeCheck(Func<GameObject, TargetArgs, float, bool> func, float max, float min = 0)
        {
            this.Condition = a => func(a.Actor, a.Target, max);// condition;
            this.Max = max;
            this.Min = min;
        }
        public override bool Evaluate(ScriptArgs args)
        {
            if (this.Condition(args))
                return true;
            args.Net.EventOccured(Message.Types.OutOfRange, args.Actor);
            return false;
        }
    }
}
