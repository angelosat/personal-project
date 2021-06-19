using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Start_a_Town_.Components.Needs;

namespace Start_a_Town_.Components
{
    public class AIAction
    {
        public Script.Types Script { get; set; }
    //    public GameObject Source { get; set; }
        public AIAdvertisement Need { get; set; }
        public AIAction(Script.Types script, Need.Types need, float value)
        {
            this.Script = script;
            this.Need = new AIAdvertisement(need, value);
        }
        public AIAction(GameObject source, Script.Types script, Need.Types need, float value)
        {
           // this.Source = source;
            this.Script = script;
            this.Need = new AIAdvertisement(need, value);
        }
    }

    class AdvertiseNeedComponent : Component
    {
        public override string ComponentName
        {
            get { return "AIReaction"; }
        }
        //public override void MakeChildOf(GameObject parent)
        //{
        //    foreach (var a in this.Actions)
        //        a.Source = parent;
        //}
        public List<AIAction> Actions { get; set; }
        public AdvertiseNeedComponent()
        {
            this.Actions = new List<AIAction>();
        }
        public AdvertiseNeedComponent Initialize(params AIAction[] reactions)
        {
            foreach (var action in reactions)
                this.Actions.Add(action);
            return this;
        }
        public override void AIQuery(GameObject parent, GameObject ai, List<AIAction> actions)
        {
            //base.AIQuery(parent, ai, actions);
            actions.AddRange(this.Actions);
        }
        public override object Clone()
        {
            return new AdvertiseNeedComponent().Initialize(this.Actions.ToArray());
        }
    }
}
