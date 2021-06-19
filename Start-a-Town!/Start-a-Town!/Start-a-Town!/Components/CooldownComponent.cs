using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Start_a_Town_.Components
{  
    class CooldownComponent : Component
    {
        public CooldownComponent()
        {
            Properties = new ComponentPropertyCollection();
            Properties.Add("Cooldown", 0f);
            Properties.Add("Speed", 1f);
            //Properties.Add("Cooldown", new ComponentProperty(0f));
            //Properties.Add("Speed", new ComponentProperty(1f));
        }

        public override void Update(GameObject parent, Chunk chunk)
        {
            Properties["Cooldown"] = Math.Max(0, GetProperty<float>("Cooldown") - GetProperty<float>("Speed") * GlobalVars.DeltaTime);
        }

        public override object Clone()
        {
            CooldownComponent comp = new CooldownComponent();
            return comp;
        }

    }
}
