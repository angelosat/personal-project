using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_.Components.Items
{
    public interface IItemFactory
    {
        GameObject Create(List<GameObjectSlot> materials);
        //GameObject Create(params GameObjectSlot[] materials);
    }
}
