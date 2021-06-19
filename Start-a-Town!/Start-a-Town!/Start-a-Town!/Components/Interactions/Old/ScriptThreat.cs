using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_.Components.Interactions
{
    class ScriptThreat :Script
    {
        public override Script.Types ID
        {
            get
            {
                return Script.Types.Threat;
            }
        }
        public override string Name
        {
            get
            {
                return "Threat";
            }
        }
        public override void OnStart(ScriptArgs args)
        {
            //base.Start(args);
            args.Net.PostLocalEvent(args.Actor, Message.Types.Threat, args.Target.Object);
            Finish(args);
        }
        public override object Clone()
        {
            return new ScriptThreat();
        }
    }
}
