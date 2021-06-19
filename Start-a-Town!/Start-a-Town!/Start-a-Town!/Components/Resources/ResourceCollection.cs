using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_.Components
{
    class ResourceCollection
    {
        Dictionary<Resource.Types, Resource> Dictionary { get; set; }
        public Resource this[Resource.Types id]
        { get { return this.Dictionary[id]; } }

        public void Update(GameObject parent)
        {
            foreach (var item in this.Dictionary.Values)
                item.Update(parent);
        }
    }
}
