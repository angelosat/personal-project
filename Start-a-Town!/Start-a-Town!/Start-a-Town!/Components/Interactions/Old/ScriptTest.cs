using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_.Components.Interactions
{
    class ScriptTest :Script
    {
        public override Script.Types ID
        {
            get
            {
                return Script.Types.Test;
            }
        }
        public override string Name
        {
            get
            {
                return "Test";
            }
        }
        public override void OnStart(ScriptArgs args)
        {
            //base.Start(args);
            args.Net.PostLocalEvent(args.Actor, Message.Types.Speak, "Test");
            Finish(args);
        }
        public override object Clone()
        {
            return new ScriptTest();
        }
    }
}
