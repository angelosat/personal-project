using System;

namespace Start_a_Town_
{
    public sealed class ResourceDef : Def
    {
        static public Progress Recovery => new(0, Ticks.TicksPerSecond, Ticks.TicksPerSecond); 

        public Type WorkerClass;
       
        public readonly float BaseMax = 100;

        public ResourceDef(string name, Type workerClass) : base(name)
        {
            this.WorkerClass = workerClass;
        }

        ResourceWorker workerCached;
        public ResourceWorker Worker => workerCached ??= (ResourceWorker)Activator.CreateInstance(this.WorkerClass);

        public string Format => "";

        public string Description => this.Worker.Description;
    }
}
