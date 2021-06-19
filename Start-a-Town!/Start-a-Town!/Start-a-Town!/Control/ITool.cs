using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Start_a_Town_.Control
{
    public interface ITool
    {
        event EventHandler<EventArgs> Destroy;
        void OnDestroy();
        void Execute();

        bool MouseLeft();
        bool MouseRight();
        bool MouseMiddle();
    }
}
