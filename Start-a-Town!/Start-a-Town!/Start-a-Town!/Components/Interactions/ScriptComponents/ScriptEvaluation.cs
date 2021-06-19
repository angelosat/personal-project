using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Start_a_Town_.Components.Interactions
{
    class ScriptEvaluation : ScriptComponent
    {
        public override string ComponentName
        {
            get { return "ScriptEvaluation"; }
        }
        public override object Clone()
        {
            return new ScriptEvaluation(this.Condition, this.Callback);//, this.Name);
        }

        public Action<ScriptArgs> Callback { get; private set; }
        public Func<ScriptArgs, bool> Condition { get; private set; }
        public string Name { get; private set; }
        public ScriptEvaluation(Func<ScriptArgs, bool> condition)//, string name = "")
        {
            this.Condition = condition;
            this.Callback = a => { };
            //this.Name = name;

        }
        public ScriptEvaluation(Func<ScriptArgs, bool> condition, Action<ScriptArgs> callback)//, string name = "")
        {
            this.Condition = condition;
            this.Callback = callback;
            //this.Name = name;
            
        }
        public ScriptEvaluation(Func<ScriptArgs, bool> condition, Message.Types failEvent)//, string name = "")
        {
            this.Condition = condition;
            this.Callback = a => a.Net.EventOccured(failEvent, a.Actor);
            //this.Name = name;

        }
        public override bool Evaluate(ScriptArgs args)
        {
            if (this.Condition(args))
                return true;
            this.Callback(args);
            return false;
        }
    }
}
