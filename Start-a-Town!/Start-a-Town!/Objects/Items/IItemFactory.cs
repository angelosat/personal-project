using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_
{
    public interface IItemFactory
    {
        Entity Create(Dictionary<string, Entity> materials);
        //GameObject Create(params GameObjectSlot[] materials);
    }
}
