using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.Components;

namespace Start_a_Town_.AI.Behaviors
{
    class BehaviorSequence : BehaviorComposite
    {
        //protected List<Behavior> Children;
        Behavior Current;
        public override string ToString()
        {
            return "Sequence: " + this.Current != null ? this.Current.ToString() : "<none>";
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
        public int Position;// { get; set; }
        public BehaviorSequence(params Behavior[] behavs)
        {
            this.Children = new List<Behavior>(behavs);
            this.Position = 0;
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
                        this.Position = 0;
                        return result;

                    case BehaviorState.Running:
                        this.Current = child;
                        return result;// BehaviorState.Success;

                    case BehaviorState.Success:
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
            return new BehaviorSequence((from child in this.Children select child.Clone() as Behavior).ToArray());
        }
        protected override void AddSaveData(SaveTag tag)
        {
            base.AddSaveData(tag);
            tag.Add(this.Position.Save("Position"));
        }
        internal override void Load(SaveTag tag)
        {
            base.Load(tag);
            tag.TryGetTagValue<int>("Position", out this.Position);
        }
        //public override bool HandleMessage(GameObject parent, ObjectEventArgs e)
        //{
        //    return base.HandleMessage(parent, e);
        //}

        //public override bool HandleMessage(GameObject parent, ObjectEventArgs e)
        //{
        //    foreach (var child in this.Children)
        //        child.HandleMessage(parent, e);
        //    return false;
        //}
    }
}
