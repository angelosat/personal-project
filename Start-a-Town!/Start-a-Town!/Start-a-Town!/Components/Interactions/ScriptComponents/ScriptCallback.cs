using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_.Components.Interactions
{
    class ScriptCallback : ScriptComponent
    {
        public override string ComponentName
        {
            get { return "ScriptCallback"; }
        }
        Action Callback;
        public ScriptCallback(Action callback)
        {
            this.Callback = callback;
        }
        public override void Success(ScriptArgs args)
        {
            base.Success(args);
            this.Callback();
        }
        public override object Clone()
        {
            return new ScriptCallback(this.Callback);
        }
    }
}
