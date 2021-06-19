using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Start_a_Town_.Components.Interactions
{
    class ScriptCondition : ScriptComponent
    {
        public override string ComponentName
        {
            get { return "ScriptCondition"; }
        }
        public override object Clone()
        {
            return new ScriptCondition(this.Condition, this.Callback, this.Name);
        }

        public Action Callback { get; private set; }
        public Func<bool> Condition { get; private set; }
        public string Name { get; private set; }

        public ScriptCondition(Func<bool> condition, Action callback, string name = "")
        {
            this.Condition = condition;
            this.Callback = callback;
            this.Name = name;
            
        }
        public override void Update(Net.IObjectProvider net, GameObject parent, Chunk chunk = null)
        {
        //    base.Update(net, parent, chunk);
        //}
        //public override void Update()
        //{
            if (Condition())
                this.Callback();
        }
    }
}
