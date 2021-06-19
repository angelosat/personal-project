using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_.Components.Interactions
{
    class ScriptModifyDurability : ScriptComponent
    {
        public override string ComponentName
        {
            get { return "ScriptModifyDurability"; }
        }
        public override object Clone()
        {
            return new ScriptModifyDurability(this.Mod);
        }

        float Mod { get; set; }
        public ScriptModifyDurability(float mod)
        {
            this.Mod = mod;
        }
        public override void Success(ScriptArgs args)
        {
            base.Success(args);
            InventoryComponent.GetHeldObject(args.Actor, a =>
            {
                ItemComponent.Modify(a.Object, item =>
                { 
                    //item.Durability.Value += this.Mod;
                    item.Durability.Add(this.Mod);
                    args.Net.EventOccured(Message.Types.DurabilityLoss, a.Object);
                });
            });

        }
    }
}
