using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_.Components.Interactions
{
    
    class ScriptMatch : ScriptComponent
    {
        public override string ComponentName
        {
            get { return "ScriptMatch"; }
        }
        public override object Clone()
        {
            return new ScriptMatch(this.Parent);
        }
        Script Parent;
        /// <summary>
        /// Checks if the target is set up as a valid target for this script.
        /// </summary>
        public ScriptMatch(Script parent)
        {
            this.Parent = parent;
        }
        public override bool Evaluate(ScriptArgs args)
        {
            if (!InteractiveComponent.HasAbility(args.Target.Object, Parent.ID))
            {
                args.Net.EventOccured(Message.Types.ScriptMismatch, args.Target.Object);
                return false;
            }
            return true;
        }
    }
}
