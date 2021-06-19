using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_.Components.Interactions
{
    class ScriptUseHeldItem : Script
    {
        //    e.Translate(r =>
        //    {
        //        TargetArgs t = TargetArgs.Read(e.Network, r);
        //        if (!this.Holding.HasValue)
        //            return;
        //        UseComponent use;
        //        if (!this.Holding.Object.TryGetComponent<UseComponent>(out use))
        //            return;
        //        parent.GetComponent<ControlComponent>().TryStartScript(use.InstantiatedScripts.FirstOrDefault(), new ScriptArgs(e.Network, parent, t));
        //    });

        public override Script.Types ID
        {
            get
            {
                return Script.Types.UseHeldItem;
            }
        }
        public override string Name
        {
            get
            {
                return "UseHeldItem";
            }
        }

        public ScriptUseHeldItem()
        {

        }

        public override void OnStart(ScriptArgs args)
        {
            //GameObjectSlot holding = args.Actor.GetComponent<GearComponent>().Holding;
            GameObjectSlot holding = args.Actor.GetComponent<HaulComponent>().Holding;

            if (!holding.HasValue)
            {
                this.Finish(args);
                return;
            }

            UseComponentOld use;
            if (!holding.Object.TryGetComponent<UseComponentOld>(out use))
            {
                this.Finish(args);
                return;
            }
            var scripts = use.InstantiatedScripts;
            //Script script = holding.Object.GetAvailableActions().FirstOrDefault();
            Script script = scripts.FirstOrDefault();
            if (!script.IsNull())
            {
                args.Actor.GetComponent<ControlComponent>().TryStartScript(script, new ScriptArgs(args.Net, args.Actor, args.Target));
            }
            this.Finish(args);

            //UseComponent use;
            //if (!holding.Object.TryGetComponent<UseComponent>(out use))
            //{
            //    this.Finish(args);
            //    return;
            //}
            //var scripts = use.InstantiatedScripts;
            //var applicableScripts = args.Target.Object.GetAvailableActions();
            //var match = (from scr in scripts where applicableScripts.FirstOrDefault(s => s.ID == scr.ID) != null select scr).FirstOrDefault();
            //if(match.IsNull())
            //{
            //    this.Finish(args);
            //    return;
            //}
            //args.Actor.GetComponent<ControlComponent>().TryStartScript(match, new ScriptArgs(args.Net, args.Actor, args.Target));
            //this.Finish(args);
        }

        public override object Clone()
        {
            return new ScriptUseHeldItem();
        }
    }
}
