using System;
using Start_a_Town_.Components;

namespace Start_a_Town_
{
    public class ComponentProps
    {
        public virtual Type CompType { get; set; }
        public T CreateComponent<T>() where T : EntityComponent
        {
            return Activator.CreateInstance(this.CompType) as T; 
        }
        public EntityComponent CreateComponent()
        {
            var comp = Activator.CreateInstance(this.CompType) as EntityComponent;
            comp.Initialize(this);
            return comp;
        }
    }
}
