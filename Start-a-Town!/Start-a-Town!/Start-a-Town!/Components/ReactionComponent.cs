using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.Components.Crafting;

namespace Start_a_Town_.Components
{
    class ReactionComponent : Component
    {
        public override string ComponentName
        {
            get { return "Reaction"; }
        }
        public override object Clone()
        {
            return new ReactionComponent() { Reaction = this.Reaction };
        }
        public Reaction Reaction { get { return (Reaction)this["Reaction"]; } set { this["Reaction"] = value; } }
        public ReactionComponent()
        {
            
        }
        public ReactionComponent Initialize(Reaction reaction)
        {
            this.Reaction = reaction;
            return this;
        }
    }
}
