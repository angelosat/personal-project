using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_.AI.Behaviors
{
    abstract class BehaviorYield : Behavior
    {
        IEnumerator<Behavior> Steps;
        int CurrentStepIndex;
        //public BehaviorYield()
        //{
        //    this.Steps = GetCurrentStep().GetEnumerator();
        //    this.Steps.MoveNext();
        //}
        protected BehaviorYield()
        {

        }
        protected virtual IEnumerable<Behavior> GetSteps() { return null; }
        protected virtual IEnumerable<Behavior> GetCurrentStep(Actor parent, AIState state) { return null; }
        public override BehaviorState Execute(Actor parent, AIState state)
        {
            if(this.Steps == null)
            {
                this.Steps = this.GetCurrentStep(parent, state).GetEnumerator();
                this.Steps.MoveNext();
                this.CurrentStepIndex++;
            }
            var current = this.Steps.Current;
            if (current != null)
            {
                var result = current.Execute(parent, state);
                switch (result)
                {
                    case BehaviorState.Running:
                        return BehaviorState.Running;

                    case BehaviorState.Success:
                        var hasNext = this.Steps.MoveNext();
                        this.CurrentStepIndex++;
                        if (!hasNext)
                            return BehaviorState.Success;
                        return BehaviorState.Running;

                    case BehaviorState.Fail:
                        return BehaviorState.Fail;
                }
            }
            return BehaviorState.Success;
        }
        public override object Clone()
        {
            throw new NotImplementedException();
        }
        //internal override SaveTag Save(string name = "")
        //{
        //    var tag = new SaveTag(SaveTag.Types.Compound, typeof(BehaviorYield).FullName);
        //    tag.Add(this.CurrentStepIndex.Save("CurrentStep"));
        //    tag.Add(this.Steps.Current.Save("Behavior"));
        //    return tag;
        //}
        public override void Write(System.IO.BinaryWriter w)
        {
            base.Write(w);
            w.Write(this.CurrentStepIndex);
            this.Steps.Current.Write(w);
        }
        public override void Read(System.IO.BinaryReader r)
        {
            base.Read(r);
            this.CurrentStepIndex = r.ReadInt32();
            
            //this.Steps.Current.Read(r);
            if(this.Steps == null)
            {
                this.Steps = this.GetSteps().GetEnumerator();
                this.Steps.MoveNext();
                for (int i = 0; i < this.CurrentStepIndex; i++)
                    this.Steps.MoveNext();
            }
            this.Steps.Current.Read(r);
        }
        protected override void AddSaveData(SaveTag tag)
        {
            //var tag = new SaveTag(SaveTag.Types.Compound, typeof(BehaviorYield).FullName);
            base.AddSaveData(tag);
            tag.Add(this.CurrentStepIndex.Save("CurrentStep"));
            tag.Add(this.Steps.Current.Save("Behavior"));
            //return tag;
        }
        internal override void Load(SaveTag tag)
        {
            base.Load(tag);

            var currentStep = tag.GetValue<int>("CurrentStep");
            this.CurrentStepIndex = currentStep;
            this.Steps = this.GetSteps().GetEnumerator();
            this.Steps.MoveNext();
            for (int i = 0; i < currentStep; i++)
            {
                this.Steps.MoveNext();
            }
            //var behavTag = tag.GetValue<SaveTag>("Behavior");
            this.Steps.Current.Load(tag["Behavior"]);
        }
        internal override void ObjectLoaded(GameObject parent)
        {
            this.Steps.Current.ObjectLoaded(parent);
        }
        //public BehaviorYield(SaveTag tag)
        //{
        //    var currentStep = tag.GetValue<int>("CurrentStep");
        //    for (int i = 0; i < currentStep; i++)
        //    {
        //        this.Steps.MoveNext();
        //    }
        //    var behavTag = tag.GetValue<SaveTag>("Behavior");
        //    this.Steps.Current.Load(behavTag);
        //}
    }
    
    //abstract class BehaviorYield
    //{
    //    IEnumerator<Behavior> Steps;
    //    public BehaviorYield()
    //    {
    //        this.Steps = GetCurrentStep().GetEnumerator();
    //        this.Steps.MoveNext();
    //    }
    //    protected abstract IEnumerable<Behavior> GetCurrentStep();
    //    public BehaviorState Execute(Entity parent, AIState state)
    //    {
    //        var current = this.Steps.Current;
    //        if (current != null)
    //        {
    //            var result = current.Execute(parent, state);
    //            switch (result)
    //            {
    //                case BehaviorState.Running:
    //                    return BehaviorState.Running;
    //                    break;

    //                case BehaviorState.Success:
    //                    var hasNext = this.Steps.MoveNext();
    //                    if (!hasNext)
    //                        return BehaviorState.Success;
    //                    return BehaviorState.Running;
    //                    break;

    //                case BehaviorState.Fail:
    //                    return BehaviorState.Fail;
    //            }
    //        }
    //        return BehaviorState.Success;
    //    }
    //}
}
