using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Start_a_Town_
{
    interface ISelectableItemList<INamed>
    {
        Action<INamed> SelectAction { set; }

    }
}
