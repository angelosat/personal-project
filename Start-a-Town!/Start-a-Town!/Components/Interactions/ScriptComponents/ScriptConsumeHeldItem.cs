using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_.Components.Interactions
{
    class ScriptConsumeHeldItem : ScriptComponent
    {
        public override string ComponentName
        {
            get { return "ScriptConsumeHeldItem"; }
        }
        public override object Clone()
        {
            return new ScriptConsumeHeldItem();
        }

        GameObject ObjectSnapshot { get; set; }

        public override void Start(ScriptArgs args)
        {
            //this.ObjectSnapshot = args.Actor.GetComponent<GearComponent>().Holding.Object;
            this.ObjectSnapshot = args.Actor.GetComponent<HaulComponent>().Holding.Object;

        }
        public override void Success(ScriptArgs args)
        {
            //GameObjectSlot currentObj = args.Actor.GetComponent<GearComponent>().Holding;
            GameObjectSlot currentObj = args.Actor.GetComponent<HaulComponent>().Holding;

            if(this.ObjectSnapshot != currentObj.Object)
            {
                return;
                // fail script
            }
            GameObject o = currentObj.Object;
            currentObj.StackSize--;
            if (!currentObj.HasValue)
                args.Net.DisposeObject(o);
        }
    }
}
