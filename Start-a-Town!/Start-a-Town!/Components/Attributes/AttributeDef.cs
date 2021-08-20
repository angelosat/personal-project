using System;

namespace Start_a_Town_
{
    public class AttributeDef : Def
    {
        readonly Type WorkerClass;
        AttributeWorker _workerCached;
        public AttributeWorker Worker => _workerCached ??= (AttributeWorker)Activator.CreateInstance(this.WorkerClass, this);
        public string Description;
        public AttributeDef(string name, Type workerClass, string description = "") : base(name)
        {
            this.WorkerClass = workerClass;
            this.Description = description;
        }
    }
}
