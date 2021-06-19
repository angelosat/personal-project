using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_.Components.Interactions
{
    class ScriptPickUpSlot : Script
    {
        public override Script.Types ID
        {
            get
            {
                return Script.Types.PickUpSlot;
            }
        }
        public override string Name
        {
            get
            {
                return "Pick Up Slot";
            }
        }
        public ScriptPickUpSlot()
        {

        }
        //public override void OnSuccess(ScriptArgs args)
        //{
        //    //base.Success(args);
        //    "pickup".ToConsole();
        //}
        //public override void Start(ScriptArgs args)
        public override void OnStart(ScriptArgs args)
        {
          //  base.Start(args);
            args.Net.PostLocalEvent(args.Actor, Message.Types.Insert, args.Target.Slot);
            Finish(args);
        }
        public override object Clone()
        {
            return new ScriptPickUpSlot();
        }
    }
}
