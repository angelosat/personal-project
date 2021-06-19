using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.Components.Needs;

namespace Start_a_Town_.Components.Consumables
{
    class NeedEffect : ConsumableEffect
    {
        Need.Types Type;
        float Value;

        public NeedEffect(Need.Types type, float value)
        {
            this.Type = type;
            this.Value = value;
        }

        public override void Apply(GameObject actor)
        {
            var need = NeedsComponent.GetNeed(actor, this.Type);
            if (need == null)
                return;
            need.Value += this.Value;
        }

        public override string ToString()
        {
            return Need.Factory.Registry[this.Type].Name + " " + this.Value.ToString("+#;-#;0");
        }
    }
}
