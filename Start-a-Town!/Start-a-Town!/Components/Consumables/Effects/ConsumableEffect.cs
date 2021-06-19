using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.Components.Needs;

namespace Start_a_Town_.Components
{
    public abstract class ConsumableEffect
    {
        //Action<GameObject> Action = a => { };
        //public virtual void Apply(GameObject actor)
        //{
        //    if (this.Action != null)
        //        this.Action(actor);
        //}
        //public Need.Types Need;
        public abstract void Apply(GameObject actor);
    }
}
