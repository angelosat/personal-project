using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Start_a_Town_.Components
{
    class TemplateComponent : Component
    {
        Action<GameObject> OnCraft { get { return (Action<GameObject>)this["OnCraft"]; } set { this["OnCraft"] = value; } }

        public TemplateComponent(Action<GameObject> onCraft)
        {
            this.OnCraft = onCraft;
        }

        public override object Clone()
        {
            return new TemplateComponent(OnCraft);
        }
    }
}
