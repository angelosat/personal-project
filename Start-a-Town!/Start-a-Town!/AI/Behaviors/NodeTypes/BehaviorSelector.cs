using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_.AI.Behaviors
{
    class BehaviorSelector : BehaviorComposite
    {
        public BehaviorSelector(params Behavior[] behavs)
        {
            this.Children = new List<Behavior>(behavs);
        }
        Behavior Current;
        int Position;
        public override string ToString()
        {
            return this.Current != null ? this.Current.ToString() : "<none>";
        }
        public override BehaviorState Execute(Actor parent, AIState state)//IObjectProvider net, GameObject parent, Personality personality, Knowledge knowledge, params object[] p)
        {
            for (int i = this.Position; i < this.Children.Count; i++)
            {
                var child = this.Children[i];
                this.Position = i;
                var result = child.Execute(parent, state);//net, parent, personality, knowledge, p);
                switch (result)
                {
                    case BehaviorState.Fail:
                        break;

                    case BehaviorState.Running:
                        this.Current = child;
                        return result;

                    case BehaviorState.Success:
                        this.Position = 0;
                        return result;
                }
            }
            this.Position = 0;
            return BehaviorState.Fail;
        }
        //public override Behavior Initialize(GameObject parent)
        //{
        //    foreach (var child in this.Children)
        //        child.Initialize(parent);
        //    return this;
        //}
        //public override Behavior Initialize(AIState state)
        //{
        //    foreach (var child in this.Children)
        //        child.Initialize(state);
        //    return this;
        //}
        public override void Write(System.IO.BinaryWriter w)
        {
            w.Write(this.Position);
            base.Write(w);
        }
        public override void Read(System.IO.BinaryReader r)
        {
            this.Position = r.ReadInt32();
            base.Read(r);
        }
        public override object Clone()
        {
            return new BehaviorSelector((from child in this.Children select child.Clone() as Behavior).ToArray());
        }
    }
}
