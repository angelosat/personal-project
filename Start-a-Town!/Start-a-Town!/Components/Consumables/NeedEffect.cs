using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.Components.Needs;

namespace Start_a_Town_.Components
{
    public class NeedEffect : ConsumableEffect
    {
        public NeedDef Type;
        public float Value;

        public NeedEffect(NeedDef type, float value)
        {
            this.Type = type;
            this.Value = value;
        }

        public override void Apply(GameObject actor)
        {
            var need = actor.GetNeed(this.Type);// NeedsComponent.GetNeed(actor, this.Type);
            if (need == null)
                return;
            need.Value += this.Value;
        }

        //public override string ToString()
        //{
        //    return Need.Factory.Registry[this.Type].Name + " " + this.Value.ToString("+#;-#;0");
        //}
    }
}
