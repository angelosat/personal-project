﻿using Start_a_Town_.UI;

namespace Start_a_Town_
{
    public interface IListable
    {
        string Label { get; }
        Control GetListControlGui();
    }
}
