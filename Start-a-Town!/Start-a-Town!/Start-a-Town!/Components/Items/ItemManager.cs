using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_.Components.Items
{
    class ItemManager
    {
        const int Offset = 50000;
        static int IDSequence = Offset;

        static public int Register()
        {
            return IDSequence++;
        }
    }
}
