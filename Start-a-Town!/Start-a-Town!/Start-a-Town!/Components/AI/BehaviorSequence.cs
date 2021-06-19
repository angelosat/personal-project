using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_.Components.AI
{
    class BehaviorSequence : Behavior
    {
        Behavior Current;
        public override string ToString()
        {
            return this.Current != null ? this.Current.ToString() : "<none>";
        }
        public override Behavior Initialize(GameObject parent)
        {
            foreach (var child in this.Children)
                child.Initialize(parent);
            return this;
        }
        public override Behavior Initialize(AIState state)
        {
            foreach (var child in this.Children)
                child.Initialize(state);
            return this;
        }
        public int Position { get; set; }
        public BehaviorSequence(params Behavior[] behavs)
            : base(behavs)
        {
            this.Position = 0;
        }
        public override BehaviorState Execute(GameObject parent, AIState state)//Net.IObjectProvider net, GameObject parent, Personality personality, Knowledge knowledge, params object[] p)
        {
            for (int i = this.Position; i < this.Children.Count; i++)
            {
                var child = this.Children[i];
                this.Position = i;
                var result = child.Execute(parent, state);//net, parent, personality, knowledge, p);
                switch (result)
                {
                    case BehaviorState.Fail:
                        this.Position = 0;
                        //this.Current = null;

                        return result;

                    case BehaviorState.Running:
                        this.Current = child;
                        return BehaviorState.Success;
                        return result;

                    case BehaviorState.Success:
                        //this.Current = null;

                        break;
                }
            }
            this.Position = 0;
            return BehaviorState.Success;
            //foreach(var child in this.Children)
            //{
            //    var result = child.Execute(net, parent, personality, knowledge, p);
            //    switch(result)
            //    {
            //        case BehaviorState.Fail:
            //            break;

            //        case BehaviorState.Running:
            //            break;

            //        case BehaviorState.Success:
            //            break;
            //    }
            //}
        }

        public override object Clone()
        {
            return new BehaviorSequence((from child in this.Children select child.Clone() as Behavior).ToArray());
        }
        public override bool HandleMessage(GameObject parent, ObjectEventArgs e)
        {
            return base.HandleMessage(parent, e);
        }

        //public override bool HandleMessage(GameObject parent, ObjectEventArgs e)
        //{
        //    foreach (var child in this.Children)
        //        child.HandleMessage(parent, e);
        //    return false;
        //}
    }
}
