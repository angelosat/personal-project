using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Start_a_Town_.Components.Spells
{
    class SpellComponent : Component
    {
        public override string ComponentName
        {
            get { return "Spell"; }
        }
        public Spell Spell { get { return (Spell)this["Spell"]; } set { this["Spell"] = value; } }

        public override object Clone()
        {
            return new SpellComponent();
        }
    }
}
