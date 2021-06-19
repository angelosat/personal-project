using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Start_a_Town_.UI;

namespace Start_a_Town_
{
    public interface ITaskGenerator
    {
        event EventHandler<TaskEventArgs> TaskOrder;
        event EventHandler<BuildEventArgs> BuildOrder;
    }
}
