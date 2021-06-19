using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Start_a_Town_.Components.Interactions
{
    /// <summary>
    /// TODO: Dont check every frame. Set up a rate, maybe 10 times a second.
    /// </summary>
    class ScriptEvaluations : ScriptComponent
    {
        public override string ComponentName
        {
            get { return "ScriptEvaluations"; }
        }
        public override object Clone()
        {
            return new ScriptEvaluations(this.Callback, this.Evaluations.ToArray());//this.Condition, this.Callback);//, this.Name);
        }
        List<ScriptEvaluation> Evaluations;
        public Action<ScriptArgs> Callback { get; private set; }

        public ScriptEvaluations(Action<ScriptArgs> callback, params ScriptEvaluation[] evals)
        {
            this.Callback = callback;
            this.Evaluations = new List<ScriptEvaluation>(evals);
        }

        public override bool Evaluate(ScriptArgs args)
        {
            foreach (var ev in this.Evaluations)
                if(!ev.Evaluate(args))
                {
                    this.Callback(args);
                    return false;
                }
            return true;
            //bool ok = true;
            //foreach (var ev in this.Evaluations)
            //    ok &= ev.Evaluate(args);
            //if (!ok)
            //    this.Callback(args);
            //return ok;
        }
    }
}
