using System;

namespace Start_a_Town_
{
    interface ISelectableItemList<INamed>
    {
        Action<INamed> SelectAction { set; }
    }
}
