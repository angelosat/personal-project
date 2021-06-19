using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.AI;

namespace Start_a_Town_.AI.Behaviors
{
    class BehaviorQueue : BehaviorComposite
    {
        //protected List<Behavior> Children;

        public BehaviorQueue(params Behavior[] behavs)
            //: base(behavs)
        {
            this.Children = new List<Behavior>(behavs);
        }
        public Behavior Current;
        //List<Behavior> Children;
        public override string ToString()
        {
            return this.Current != null ? this.Current.ToString() : "<none>";
        }
        public override BehaviorState Execute(Actor parent, AIState state)
        {
            
            foreach (var child in this.Children)
            {
                var result = child.Execute(parent, state);
                // dont traverse next child if the current child is running
                //if (result == BehaviorState.Success)
                //{
                //    this.Current = child;
                //    return BehaviorState.Success;
                //}

                if (result != BehaviorState.Fail)
                {
                    this.Current = child;
                    return result;
                }
            }
            this.Current = null;
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
        public override object Clone()
        {
            return new BehaviorQueue((from child in this.Children select child.Clone() as Behavior).ToArray());
        }

        //public override bool HandleMessage(GameObject parent, ObjectEventArgs e)
        //{
        //    foreach (var child in this.Children)
        //        child.HandleMessage(parent, e);
        //    return false;
        //}
        //protected override void AddSaveData(SaveTag tag)
        //{
        //    //var tag = new SaveTag(SaveTag.Types.Compound, name);
        //    //tag.Add((this.Children.IndexOf(this.Current)).Save("Index"));
        //    base.AddSaveData(tag);

        //    var childrenTag = new SaveTag(SaveTag.Types.List, "Children", SaveTag.Types.Compound);
        //    foreach (var c in this.Children)
        //    {
        //        var ct = c.Save(c.Name);
        //        childrenTag.Add(ct);//new SaveTag(SaveTag.Types.Compound, c.Name, ct));
        //    }
        //    tag.Add(childrenTag);
        //    //return tag;
        //}
        //internal override void Load(SaveTag tag)
        //{
        //    //tag.TryGetTagValue<int>("Index", i => this.Current = this.Children[i]);
        //    base.Load(tag);

        //    var childTags = tag["Children"].Value as List<SaveTag>;
        //    for (int i = 0; i < this.Children.Count; i++)
        //    {
        //        this.Children[i].Load(childTags[i]);
        //    }
        //}
        internal override void MapLoaded(Actor parent)
        {
            foreach (var ch in this.Children)
                ch.MapLoaded(parent);
        }
    }
}
