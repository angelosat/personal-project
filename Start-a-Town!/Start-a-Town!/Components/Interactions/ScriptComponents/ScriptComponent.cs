using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;

namespace Start_a_Town_.Components.Interactions
{
    public abstract class ScriptComponent : EntityComponent
    {
        //public virtual ScriptState Update(ScriptArgs args) { return ScriptState.Finished; }
        public virtual void Start(ScriptArgs args) { }
        public virtual void Finish(ScriptArgs args) { }
        public virtual void Success(ScriptArgs args) { }
        public virtual bool Evaluate(ScriptArgs args) { return true; }
    }
}
