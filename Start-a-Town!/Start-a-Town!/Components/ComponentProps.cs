using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.Components;

namespace Start_a_Town_
{
    public class ComponentProps
    {
        //public abstract Type CompType { get; }
        public virtual Type CompType { get; set; }
        //public ComponentProps(Type compType)
        //{
        //    this.CompType = compType;
        //}
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
