using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.Components.Interactions;

namespace Start_a_Town_.Components
{
    class UseComponent : Component
    {
        public override string ComponentName
        {
            get { return "Use"; }
        }

        public Interaction Interaction { get { return (Interaction)this["Interaction"]; } set { this["Interaction"] = value; } }

        public UseComponent()
        {

        }
        public UseComponent(Interaction interaction)
        {
            this.Interaction = interaction;
        }

        static public Interaction GetInteraction(GameObject obj)
        {
            UseComponent use;
            if (!obj.TryGetComponent<UseComponent>(out use))
                return null;
            return use.Interaction.Clone() as Interaction;
        }

        public override object Clone()
        {
            return new UseComponent(this.Interaction);
        }
    }
}
