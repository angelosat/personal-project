using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.Components.Interactions
{
    class ScriptDrop : Script
    {
        public override Script.Types ID
        {
            get
            {
                return Types.Drop;
            }
        }
        public override string Name
        {
            get
            {
                return "Drop";
            }
        }

        public ScriptDrop()
        {
            //this.AddComponent(new ScriptTargetFilter(t => t.IsBlock()));
        }

        public override void OnStart(ScriptArgs args)
        {
            //args.Actor.GetComponent<GearComponent>().Throw(Vector3.Zero, args.Actor);
            args.Actor.GetComponent<HaulComponent>().Throw(Vector3.Zero, args.Actor);

            //args.Actor.GetComponent<ControlComponent>().FinishScript(Types.Hauling, args);
            Finish(args);
        }
        public override object Clone()
        {
            return new ScriptDrop();
        }
    }
}
