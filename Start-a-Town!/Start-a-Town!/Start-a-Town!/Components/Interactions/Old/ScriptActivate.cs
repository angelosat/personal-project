using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Start_a_Town_.Components.Interactions
{
    class ScriptActivate : Script
    {
        public override Script.Types ID
        {
            get
            {
                return Script.Types.Activate;
            }
        }
        public override string Name
        {
            get
            {
                return "Activate";
            }
        }
        public override void OnStart(ScriptArgs args)
        {
            switch (args.Target.Type)
            {
                case Start_a_Town_.TargetType.Entity:
                    args.Net.PostLocalEvent(args.Target.Object, ObjectEventArgs.Create(Message.Types.Activate, new object[] { args.Actor }));
                    break;

                case Start_a_Town_.TargetType.Position:
                    //Block.HandleMessage(args.Net, args.Target.Global, ObjectEventArgs.Create(args.Net, Message.Types.Activate, args.Actor));
                    args.Net.SendBlockMessage(args.Target.Global, Message.Types.Activate, args.Actor);
                    break;

                default:
                    break;
            }
        Finish(args);
        }
        public override object Clone()
        {
            return new ScriptActivate();
        }
    }
}
